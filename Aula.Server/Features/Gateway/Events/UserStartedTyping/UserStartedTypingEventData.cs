using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Features.Gateway.Events.UserStartedTyping;

/// <summary>
///     Occurs when a user is typing a message.
/// </summary>
internal sealed record UserStartedTypingEventData
{
	/// <summary>
	///     The ID of the user typing.
	/// </summary>
	public required Snowflake UserId { get; init; }

	/// <summary>
	///     The room where the user is typing.
	/// </summary>
	public required Snowflake RoomId { get; init; }
}
