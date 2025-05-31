using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Domain.Rooms;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Snowflakes;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Domain.Messages;

[Owned]
internal class MessageUserLeave
{
	[SetsRequiredMembers]
	internal MessageUserLeave(Snowflake userId, Snowflake? previousRoomId)
	{
		UserId = userId;
		NextRoomId = previousRoomId;
	}

	// EntityFramework constructor
	private MessageUserLeave()
	{ }

	public required Snowflake UserId { get; init; }

	public Snowflake? NextRoomId { get; init; }

	[ForeignKey(nameof(UserId))]
	public virtual User User { get; init; } = null!;

	[ForeignKey(nameof(NextRoomId))]
	public virtual Room? NextRoom { get; init; }
}
