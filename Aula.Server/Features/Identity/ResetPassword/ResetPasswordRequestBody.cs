namespace Aula.Server.Features.Identity.ResetPassword;

/// <summary>
///     Holds the data required to reset a user's password.
/// </summary>
internal sealed record ResetPasswordRequestBody
{
	/// <summary>
	///     The token used to validate the password reset request.
	/// </summary>
	public required String Code { get; init; }

	/// <summary>
	///     The new password to be set for the user.
	/// </summary>
	public required String NewPassword { get; init; }
}
