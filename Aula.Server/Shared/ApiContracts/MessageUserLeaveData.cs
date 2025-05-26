using Aula.Server.Domain.Messages;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Shared.ApiContracts;

/// <summary>
///     Holds data required by <see cref="MessageType.UserLeave" /> messages.
/// </summary>
internal sealed record MessageUserLeaveData
{
	/// <summary>
	///     The ID of user who moved out of the room.
	/// </summary>
	public required Snowflake UserId { get; init; }

	/// <summary>
	///     The ID of the room where it resides now.
	/// </summary>
	public required Snowflake? NextRoomId { get; init; }
}
