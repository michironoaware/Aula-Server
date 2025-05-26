using Aula.Server.Domain.Rooms;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Features.Rooms.CreateRoom;

/// <summary>
///     Holds the data required to create a new room.
/// </summary>
internal sealed record CreateRoomRequestBody
{
	/// <summary>
	///     The type of the room.
	/// </summary>
	public required RoomType Type { get; init; }

	/// <summary>
	///     The name of the room.
	/// </summary>
	public required String Name { get; init; }

	/// <summary>
	///     The description of the room.
	/// </summary>
	public String? Description { get; init; }

	/// <summary>
	///     Whether the room serves as an entry point for users without an established current room.
	/// </summary>
	public Boolean? IsEntrance { get; init; }

	/// <summary>
	///     The file id of the background audio for the room.
	/// </summary>
	public Snowflake? BackgroundAudioId { get; init; }
}
