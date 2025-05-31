using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Rooms;

internal class RoomConnection : DomainEntity
{
	[SetsRequiredMembers]
	internal RoomConnection(Snowflake id, Snowflake sourceRoomId, Snowflake destinationRoomId)
	{
		Id = id;
		SourceRoomId = sourceRoomId;
		DestinationRoomId = destinationRoomId;
	}

	// EntityFramework constructor
	private RoomConnection()
	{ }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public required Snowflake Id { get; init; }

	public required Snowflake SourceRoomId { get; init; }

	public required Snowflake DestinationRoomId { get; init; }

	[ForeignKey(nameof(SourceRoomId))]
	public virtual Room SourceRoom { get; init; } = null!;

	[ForeignKey(nameof(DestinationRoomId))]
	public virtual Room TargetRoom { get; init; } = null!;
}
