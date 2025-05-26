using System.Diagnostics.CodeAnalysis;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Messages;

internal class UserLeaveMessage : Message
{
	[SetsRequiredMembers]
	internal UserLeaveMessage(
		Snowflake id,
		MessageFlags flags,
		Snowflake? authorId,
		Snowflake roomId,
		MessageUserLeave leaveData)
		: base(id, MessageType.UserLeave, flags, authorId, roomId)
	{
		LeaveData = leaveData;
	}

	// EntityFramework constructor
	private UserLeaveMessage()
	{ }

	public virtual required MessageUserLeave LeaveData { get; init; }
}
