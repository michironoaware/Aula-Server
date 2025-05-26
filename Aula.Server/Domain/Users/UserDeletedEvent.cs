namespace Aula.Server.Domain.Users;

internal sealed record UserDeletedEvent(User User) : DomainEvent;
