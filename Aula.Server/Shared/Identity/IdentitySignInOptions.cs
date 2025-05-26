namespace Aula.Server.Shared.Identity;

/// <summary>
///     Sign in related configurations.
/// </summary>
internal sealed class IdentitySignInOptions
{
	/// <summary>
	///     Whether the user required a confirmed email to log into the application.
	/// </summary>
	public Boolean RequireConfirmedEmail { get; set; } = true;
}
