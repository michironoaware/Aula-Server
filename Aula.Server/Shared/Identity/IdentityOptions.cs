namespace Aula.Server.Shared.Identity;

/// <summary>
///     User identity related configurations.
/// </summary>
internal sealed class IdentityOptions
{
	internal const String SectionName = "Identity";

	/// <inheritdoc cref="IdentitySignInOptions" />
	public IdentitySignInOptions SignIn { get; set; } = new();

	/// <inheritdoc cref="IdentityPasswordOptions" />
	public IdentityPasswordOptions Password { get; set; } = new();

	/// <inheritdoc cref="IdentityLockoutOptions" />
	public IdentityLockoutOptions Lockout { get; set; } = new();
}
