using Aula.Server.Domain.Users;
using Aula.Server.Features.Identity.Shared;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Tokens;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Identity.ResetPassword;

internal sealed class ResetPasswordEmailSender
{
	private readonly IEmailSender _emailSender;
	private readonly Uri? _redirectUri;
	private readonly ITokenProvider _tokenProvider;
	private readonly UserManager _userManager;

	public ResetPasswordEmailSender(
		[FromServices] UserManager userManager,
		[FromServices] IEmailSender emailSender,
		[FromServices] IOptions<IdentityFeatureOptions> featureOptions,
		[FromServices] ITokenProvider tokenProvider)
	{
		_userManager = userManager;
		_emailSender = emailSender;
		_tokenProvider = tokenProvider;
		_redirectUri = featureOptions.Value.ResetPasswordRedirectUri;
	}

	internal async Task SendEmailAsync(StandardUser user)
	{
		if (user.Email is null)
			throw new ArgumentException("The user email address cannot be null.", nameof(user));

		var resetToken = _userManager.GeneratePasswordResetToken(user);
		var code = _tokenProvider.CreateToken(user.Id.ToString(), resetToken);
		var content =
			$"""
			<p>Hello! did you forget your password? Here's your reset password code: <code>{code}</code></p>
			<p>If you didn't request a password reset, you can ignore this email.</p>
			""";
		if (_redirectUri is not null)
			content += $"<p>You can reset your password by <a href='{_redirectUri}'>clicking here</a>.</p>";

		await _emailSender.SendEmailAsync(user.Email, "Reset your password", content);
	}
}
