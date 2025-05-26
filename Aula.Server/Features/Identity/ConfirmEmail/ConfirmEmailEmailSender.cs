using System.Text;
using Aula.Server.Domain.Users;
using Aula.Server.Shared;
using Aula.Server.Shared.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Identity.ConfirmEmail;

internal sealed class ConfirmEmailEmailSender
{
	private readonly String _applicationName;
	private readonly IEmailSender _emailSender;
	private readonly UserManager _userManager;

	public ConfirmEmailEmailSender(
		[FromServices] UserManager userManager,
		[FromServices] IEmailSender emailSender,
		[FromServices] IOptions<ApplicationOptions> applicationOptions)
	{
		_userManager = userManager;
		_emailSender = emailSender;
		_applicationName = applicationOptions.Value.Name;
	}

	internal async Task SendEmailAsync(StandardUser user)
	{
		if (user.Email is null)
			throw new ArgumentException("The user email address cannot be null.", nameof(user));

		var confirmationToken = _userManager.GenerateEmailConfirmationToken(user);
		confirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));
		var content =
			$"""
			<p>Hello {user.UserName}, Welcome to {_applicationName}!</p>
			<p>Here's your email confirmation token: <code>{confirmationToken}</code>
			<p>If you didn’t sign up for {_applicationName}, you can ignore this email.</p>
			""";
		await _emailSender.SendEmailAsync(user.Email, "Confirm your email", content);
	}
}
