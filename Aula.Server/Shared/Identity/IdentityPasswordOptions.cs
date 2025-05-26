namespace Aula.Server.Shared.Identity;

/// <summary>
///     User passwords related configurations.
/// </summary>
internal sealed class IdentityPasswordOptions
{
	/// <summary>
	///     Whether the password requires a UPPERCASE character.
	/// </summary>
	public Boolean RequireUppercase { get; set; } = true;

	/// <summary>
	///     Whether the password requires a lowercase character.
	/// </summary>
	public Boolean RequireLowercase { get; set; } = true;

	/// <summary>
	///     Whether the password requires a digit character.
	/// </summary>
	public Boolean RequireDigit { get; set; } = true;

	/// <summary>
	///     Whether the password requires a non-alphanumeric character.
	/// </summary>
	public Boolean RequireNonAlphanumeric { get; set; } = true;

	/// <summary>
	///     How many different character the password should have.
	/// </summary>
	public Int32 RequiredUniqueChars { get; set; }

	/// <summary>
	///     The minimum length a password requires.
	/// </summary>
	public Int32 RequiredLength { get; set; } = 8;
}
