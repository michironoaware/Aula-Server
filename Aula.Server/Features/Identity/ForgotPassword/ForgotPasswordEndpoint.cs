using System.Buffers.Text;
using System.Text;
using Aula.Server.Features.Identity.ResetPassword;
using Aula.Server.Features.Identity.Shared;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace Aula.Server.Features.Identity.ForgotPassword;

internal sealed class ForgotPasswordEndpoint : IApiEndpoint
{
	private const String EmailQueryParameter = "email";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/forgot-password", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = EmailQueryParameter)] String email,
		[FromServices] UserManager userManager,
		[FromServices] ResetPasswordEmailSender resetPasswordEmailSender)
	{
		if (!Base64Url.IsValid(email))
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidBase64UrlEmail);
		email = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(email));

		var user = await userManager.FindByEmailAsync(email);
		if (user is null)
		{
			// We could return NotFound, but it feels like unnecessary information.
			return TypedResults.NoContent();
		}

		await resetPasswordEmailSender.SendEmailAsync(user);
		return TypedResults.NoContent();
	}
}
