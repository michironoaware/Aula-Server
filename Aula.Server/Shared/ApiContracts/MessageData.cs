using Aula.Server.Domain.Messages;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Shared.ApiContracts;

/// <summary>
///     Represents a message within a room.
/// </summary>
internal sealed record MessageData
{
	/// <summary>
	///     The unique identifier of the message.
	/// </summary>
	public required Snowflake Id { get; init; }

	/// <summary>
	///     The type of the message, defines the message's data.
	/// </summary>
	public required MessageType Type { get; init; }

	/// <summary>
	///     The flags of the message, indicates trivial configurations clients should take into account.
	/// </summary>
	public required MessageFlags Flags { get; init; }

	/// <summary>
	///     The type of author who sent the message.
	/// </summary>
	public required MessageAuthorType AuthorType { get; init; }

	/// <summary>
	///     The ID of the author who created the message.
	/// </summary>
	public Snowflake? AuthorId { get; init; }

	/// <summary>
	///     The ID of the message's room.
	/// </summary>
	public required Snowflake RoomId { get; init; }

	/// <summary>
	///     The text content of the message.
	/// </summary>
	public String? Text { get; init; }

	/// <summary>
	///     The room join data associated with the message. Only present for <see cref="MessageType.UserJoin" />
	/// </summary>
	public MessageUserJoinData? JoinData { get; init; }

	/// <summary>
	///     The room leave data associated with the message. Only present for <see cref="MessageType.UserLeave" />
	/// </summary>
	public MessageUserLeaveData? LeaveData { get; init; }

	/// <summary>
	///     The date and time when the message was created.
	/// </summary>
	public required DateTime CreationDate { get; init; }
}
