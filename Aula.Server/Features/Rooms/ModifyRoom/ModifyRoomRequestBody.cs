using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Features.Rooms.ModifyRoom;

/// <summary>
///     Holds the data required to update an already existing room.
/// </summary>
internal sealed record ModifyRoomRequestBody
{
	/// <summary>
	///     The new name for the room.
	/// </summary>
	public String? Name { get; init; }

	/// <summary>
	///     The new description for the room.
	/// </summary>
	public String? Description { get; init; }

	/// <summary>
	///     Indicates whether the room serves as an entry point for users without an established current room.
	/// </summary>
	public Boolean? IsEntrance { get; init; }

	/// <summary>
	///     A collection of Ids of the target rooms to connect this room with.
	/// </summary>
	public IReadOnlyList<Snowflake>? DestinationIds { get; init; }

	/// <summary>
	///     The file id of the background audio for the room.
	/// </summary>
	public Snowflake? BackgroundAudioId { get; init; }
}
