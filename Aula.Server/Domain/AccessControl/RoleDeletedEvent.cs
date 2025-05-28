namespace Aula.Server.Domain.AccessControl;

internal sealed record RoleDeletedEvent(Role Role) : DomainEvent;
