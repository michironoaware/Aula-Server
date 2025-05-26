using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Users;

internal sealed record UserSecurityStampUpdatedEvent(Snowflake UserId) : DomainEvent;
