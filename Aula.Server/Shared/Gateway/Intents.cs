namespace Aula.Server.Shared.Gateway;

/// <summary>
///     Gateway intents define which events will be dispatched during the session.
/// </summary>
[Flags]
internal enum Intents : UInt64
{
	Messages = 1 << 0,
	Moderation = 1 << 1,
}
