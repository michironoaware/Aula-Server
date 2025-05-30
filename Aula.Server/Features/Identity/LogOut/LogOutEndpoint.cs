using Aula.Server.Domain.Users;
using Aula.Server.Features.Identity.Shared;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Identity.LogOut;

internal sealed class LogOutEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/log-out", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, EmptyHttpResult>> HandleAsync(
		[FromBody] LogInRequestBody body,
		[FromServices] IValidator<LogInRequestBody> bodyValidator,
		[FromServices] UserManager userManager,
		[FromServices] AppDbContext dbContext)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var user = await dbContext.Users
			.AsTracking()
			.OfType<StandardUser>()
			.Where(u => u.UserName == body.UserName && !u.IsDeleted)
			.FirstOrDefaultAsync();
		if (user is null)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidUserName);

		var isPasswordCorrect = userManager.CheckPassword(user, body.Password);
		if (!isPasswordCorrect)
		{
			await userManager.AccessFailedAsync(user);
			return TypedResults.Problem(ProblemDetailsDefaults.IncorrectPassword);
		}

		if (user.LockoutEndTime > DateTime.UtcNow)
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsLockedOut);

		if (userManager.Options.SignIn.RequireConfirmedEmail &&
		    !user.EmailConfirmed)
			return TypedResults.Problem(ProblemDetailsDefaults.EmailNotConfirmed);

		user.SecurityStamp = Guid.CreateVersion7().ToString("N");
		_ = dbContext.SaveChangesWithConcurrencyByPassAsync();

		return TypedResults.NoContent();
	}
}
