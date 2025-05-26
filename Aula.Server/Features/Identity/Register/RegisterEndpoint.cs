using Aula.Server.Domain.Users;
using Aula.Server.Features.Identity.ConfirmEmail;
using Aula.Server.Features.Identity.ResetPassword;
using Aula.Server.Features.Identity.Shared;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Snowflakes;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Identity.Register;

internal sealed class RegisterEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/register", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromBody] RegisterRequestBody body,
		[FromServices] IValidator<RegisterRequestBody> bodyValidator,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] UserManager userManager,
		[FromServices] IPasswordHasher<User> passwordHasher,
		[FromServices] IOptions<IdentityFeatureOptions> featureOptions,
		[FromServices] ConfirmEmailEmailSender confirmEmailEmailSender,
		[FromServices] ResetPasswordEmailSender resetPasswordEmailSender)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var currentUser = await userManager.FindByEmailAsync(body.Email);
		if (currentUser is not null)
		{
			await resetPasswordEmailSender.SendEmailAsync(currentUser);
			return TypedResults.NoContent();
		}

		var id = await snowflakes.GenerateAsync();
		var displayName = body.DisplayName ?? body.UserName;
		var newUser = new StandardUser(id, displayName, String.Empty, body.UserName, body.Email);
		newUser.PasswordHash = passwordHasher.HashPassword(newUser, body.Password);

		var registerResult = await userManager.RegisterAsync(newUser);
		if (!registerResult.Succeeded)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Register problem",
				Detail = registerResult.Description,
				Status = StatusCodes.Status400BadRequest,
			});
		}

		await confirmEmailEmailSender.SendEmailAsync(newUser);
		return TypedResults.NoContent();
	}
}
