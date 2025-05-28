namespace Aula.Server.Shared.Gateway;

/// <summary>
///     Gateway intents define which events will be dispatched during the session.
/// </summary>
[Flags]
internal enum Intents : UInt64
{
	Users = 1 << 0,
	Rooms = 1 << 1,
	Messages = 1 << 2,
	Moderation = 1 << 3,
}
