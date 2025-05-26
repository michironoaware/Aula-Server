namespace Aula.Server.Features.Identity.Register;

/// <summary>
///     Holds the data required to register a new user.
/// </summary>
internal sealed record RegisterRequestBody
{
	/// <summary>
	///     The display name for this user. Defaults to the <see cref="UserName">userName</see>.
	/// </summary>
	public String? DisplayName { get; init; }

	/// <summary>
	///     A unique identifier for the user, required for signing in. Once set it cannot be modified.
	/// </summary>
	public required String UserName { get; init; }

	/// <summary>
	///     The email address for the user.
	/// </summary>
	public required String Email { get; init; }

	/// <summary>
	///     The password for the user.
	/// </summary>
	public required String Password { get; init; }
}
