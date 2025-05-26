namespace Aula.Server.Domain.Messages;

/// <summary>
///     Enumerates behaviors that can be associated with messages.
/// </summary>
[Flags]
internal enum MessageFlags : UInt64
{
	/// <summary>
	///     The author of the message should be hidden.
	/// </summary>
	HideAuthor = 1 << 0,
}
