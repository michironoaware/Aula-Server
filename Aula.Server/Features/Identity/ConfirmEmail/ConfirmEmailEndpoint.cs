using System.Buffers.Text;
using System.Text;
using Aula.Server.Features.Identity.Shared;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Identity.ConfirmEmail;

internal sealed class ConfirmEmailEndpoint : IApiEndpoint
{
	internal const String EmailQueryParamName = "email";
	internal const String TokenQueryParamName = "token";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/confirm-email", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, RedirectHttpResult, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = EmailQueryParamName)] String email,
		[FromQuery(Name = TokenQueryParamName)] String? token,
		[FromServices] UserManager userManager,
		[FromServices] ConfirmEmailEmailSender confirmEmailEmailSender,
		[FromServices] IOptions<IdentityFeatureOptions> featureOptions)
	{
		if (!Base64Url.IsValid(email))
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidBase64UrlEmail);
		email = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(email));

		var redirectUri = featureOptions.Value.ConfirmEmailRedirectUri;

		var user = await userManager.FindByEmailAsync(email);
		if (user is null)
		{
			// We could return NotFound, but it feels like unnecessary information.
			return RedirectOrSendNoContent(redirectUri);
		}

		if (user.EmailConfirmed)
			return RedirectOrSendNoContent(redirectUri);

		if (token is null)
		{
			await confirmEmailEmailSender.SendEmailAsync(user);
			return RedirectOrSendNoContent(redirectUri);
		}

		try
		{
			token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
		}
		catch (FormatException)
		{
			return RedirectOrSendNoContent(redirectUri);
		}

		_ = await userManager.ConfirmEmailAsync(user, token);
		return RedirectOrSendNoContent(redirectUri);
	}

	private static Results<NoContent, RedirectHttpResult, ProblemHttpResult> RedirectOrSendNoContent(
		Uri? redirectUri) =>
		redirectUri is not null ? TypedResults.Redirect(redirectUri.ToString()) : TypedResults.NoContent();
}
