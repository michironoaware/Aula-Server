using System.Diagnostics.CodeAnalysis;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Messages;

internal class UserJoinMessage : Message
{
	[SetsRequiredMembers]
	internal UserJoinMessage(
		Snowflake id,
		MessageFlags flags,
		Snowflake? authorId,
		Snowflake roomId,
		MessageUserJoin joinData)
		: base(id, MessageType.UserJoin, flags, authorId, roomId)
	{
		JoinData = joinData;
	}

	// EntityFramework constructor
	private UserJoinMessage()
	{ }

	public virtual required MessageUserJoin JoinData { get; init; }
}
