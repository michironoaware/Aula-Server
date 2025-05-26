using Aula.Server.Domain.Rooms;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Shared.ApiContracts;

/// <summary>
///     Represents a room within the application.
/// </summary>
internal sealed record RoomData
{
	/// <summary>
	///     The unique identifier of the room.
	/// </summary>
	public required Snowflake Id { get; init; }

	/// <summary>
	///     The type of this room.
	/// </summary>
	public required RoomType Type { get; init; }

	/// <summary>
	///     The name of the room.
	/// </summary>
	public required String Name { get; init; }

	/// <summary>
	///     The description of the room.
	/// </summary>
	public required String Description { get; init; }

	/// <summary>
	///     Whether the room serves as an entry point for users without an established current room.
	/// </summary>
	public required Boolean IsEntrance { get; init; }

	/// <summary>
	///     The file id of the background audio of this room.
	/// </summary>
	public Snowflake? BackgroundAudioId { get; init; }

	/// <summary>
	///     A collection of ids of all users that currently reside in this room.
	/// </summary>
	public required IEnumerable<Snowflake> ResidentIds { get; init; }

	/// <summary>
	///     A collection of ids of all rooms a user can go from this room.
	/// </summary>
	public required IEnumerable<Snowflake> DestinationIds { get; init; }

	/// <summary>
	///     The date and time when the room was created.
	/// </summary>
	public required DateTime CreationDate { get; init; }
}
