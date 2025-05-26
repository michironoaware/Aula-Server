namespace Aula.Server.Domain.Users;

/// <summary>
///     Defines the owner types that a character can have.
/// </summary>
internal enum UserType
{
	/// <summary>
	///     The character is owned by a standard user.
	/// </summary>
	Standard = 0,

	/// <summary>
	///     The character is owned by a bot user.
	/// </summary>
	Bot = 1,
}
