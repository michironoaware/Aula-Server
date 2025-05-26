using Aula.Server.Domain.Users;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using Aula.Server.Shared.Tokens;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Features.Bots.CreateBot;

internal sealed class CreateBotEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("bots", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<CreateBotResponseBody>, ProblemHttpResult>> HandleAsync(
		[FromBody] CreateBotRequestBody body,
		[FromServices] IValidator<CreateBotRequestBody> bodyValidator,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] ITokenProvider tokenProvider,
		[FromServices] AppDbContext dbContext,
		HttpContext httpContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var validation = await bodyValidator.ValidateAsync(body, ct);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var user = new BotUser(await snowflakes.GenerateAsync(), body.DisplayName, String.Empty);
		_ = dbContext.Users.Add(user);
		_ = await dbContext.SaveChangesAsync(ct);

		return TypedResults.Ok(new CreateBotResponseBody
		{
			User = user.ToUserData(), Token = tokenProvider.CreateToken(user),
		});
	}
}
