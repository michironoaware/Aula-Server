using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Users;

internal sealed record UserCurrentRoomUpdatedEvent(
	Snowflake UserId,
	Snowflake? PreviousRoomId,
	Snowflake? CurrentRoomId) : DomainEvent;
