using Aula.Server.Domain.Messages;

namespace Aula.Server.Features.Messages.SendMessage;

/// <summary>
///     Holds the data required to send a message.
/// </summary>
internal sealed record SendMessageRequestBody
{
	/// <summary>
	///     The type of the message.
	/// </summary>
	public required MessageType Type { get; init; }

	/// <summary>
	///     The flags of the message.
	/// </summary>
	public MessageFlags? Flags { get; init; }

	/// <summary>
	///     The text content of the message. Required for <see cref="MessageType.Default" /> messages.
	/// </summary>
	public String? Text { get; init; }
}
