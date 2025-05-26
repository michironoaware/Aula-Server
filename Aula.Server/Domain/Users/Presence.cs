namespace Aula.Server.Domain.Users;

/// <summary>
///     Defines presence statuses for a user.
/// </summary>
internal enum Presence
{
	/// <summary>
	///     The user is offline.
	/// </summary>
	Offline = 0,

	/// <summary>
	///     The user is online.
	/// </summary>
	Online = 1,
}
