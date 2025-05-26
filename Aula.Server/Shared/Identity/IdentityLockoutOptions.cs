namespace Aula.Server.Shared.Identity;

/// <summary>
///     User lockout related configurations.
/// </summary>
internal sealed class IdentityLockoutOptions
{
	/// <summary>
	///     The number of minutes to lockout users.
	/// </summary>
	public Int32 LockoutMinutes { get; set; } = 15;

	/// <summary>
	///     The maximum failed access attempts before locking out a user.
	/// </summary>
	public Int32 MaximumFailedAccessAttempts { get; set; } = 10;
}
