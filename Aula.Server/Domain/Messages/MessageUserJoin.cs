using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Domain.Rooms;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Snowflakes;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Domain.Messages;

[Owned]
internal class MessageUserJoin
{
	[SetsRequiredMembers]
	internal MessageUserJoin(Snowflake userId, Snowflake? previousRoomId)
	{
		UserId = userId;
		PreviousRoomId = previousRoomId;
	}

	// EntityFramework constructor
	private MessageUserJoin()
	{ }

	public required Snowflake UserId { get; init; }

	public Snowflake? PreviousRoomId { get; init; }

	[ForeignKey(nameof(UserId))]
	public virtual User User { get; init; } = null!;

	[ForeignKey(nameof(PreviousRoomId))]
	public virtual Room? PreviousRoom { get; init; }
}
