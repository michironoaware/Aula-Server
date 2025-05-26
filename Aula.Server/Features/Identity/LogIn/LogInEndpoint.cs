using Aula.Server.Domain.Users;
using Aula.Server.Features.Identity.Shared;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Tokens;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Features.Identity.LogIn;

internal sealed class LogInEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/log-in", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<LogInResponseBody>, ProblemHttpResult, EmptyHttpResult>> HandleAsync(
		[FromBody] LogInRequestBody body,
		[FromServices] IValidator<LogInRequestBody> bodyValidator,
		[FromServices] UserManager userManager,
		[FromServices] ITokenProvider tokenProvider)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var user = await userManager.FindByUserNameAsync(body.UserName);
		if (user?.Type is not UserType.Standard)
			return TypedResults.Problem(ProblemDetailsDefaults.UserDoesNotExist);

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

		return TypedResults.Ok(new LogInResponseBody { Token = tokenProvider.CreateToken(user) });
	}
}
