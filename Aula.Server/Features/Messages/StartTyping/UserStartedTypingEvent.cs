using Aula.Server.Shared.Snowflakes;
using MediatR;

namespace Aula.Server.Features.Messages.StartTyping;

internal sealed record UserStartedTypingEvent : INotification
{
	public required Snowflake UserId { get; init; }

	public required Snowflake RoomId { get; init; }
}
