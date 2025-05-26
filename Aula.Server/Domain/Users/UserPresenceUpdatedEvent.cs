using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Users;

internal sealed record UserPresenceUpdatedEvent(Snowflake UserId, Presence Presence) : DomainEvent;
