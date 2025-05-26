using Aula.Server.Domain.Messages;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Shared.ApiContracts;

/// <summary>
///     Holds data required by <see cref="MessageType.UserJoin" /> messages.
/// </summary>
internal sealed record MessageUserJoinData
{
	/// <summary>
	///     The ID of user who joined to this room.
	/// </summary>
	public required Snowflake UserId { get; init; }

	public Snowflake? PreviousRoomId { get; init; }
}
