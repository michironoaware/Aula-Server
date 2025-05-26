using Aula.Server.Domain.Users;
using Aula.Server.Features.Identity.Shared;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Snowflakes;
using Aula.Server.Shared.Tokens;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Features.Identity.ResetPassword;

internal sealed class ResetPasswordEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/reset-password", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromBody] ResetPasswordRequestBody body,
		[FromServices] IValidator<ResetPasswordRequestBody> bodyValidator,
		[FromServices] UserManager userManager,
		[FromServices] ITokenProvider tokenProvider)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		if (!tokenProvider.TryReadFromToken(body.Code, out var userIdString, out var resetToken) ||
		    !Snowflake.TryParse(userIdString, out var userId))
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidResetPasswordToken);

		if (await userManager.FindByIdAsync(userId) is not StandardUser user)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidResetPasswordToken);

		var passwordReset = await userManager.ResetPasswordAsync(user, body.NewPassword, resetToken);
		if (passwordReset == ResetPasswordResult.InvalidToken)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidResetPasswordToken);

		if (!passwordReset.Succeeded)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Password reset problem",
				Detail = passwordReset.Description,
				Status = StatusCodes.Status400BadRequest,
			});
		}

		return TypedResults.NoContent();
	}
}
