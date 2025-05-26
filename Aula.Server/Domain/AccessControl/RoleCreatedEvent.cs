namespace Aula.Server.Domain.AccessControl;

internal sealed record RoleCreatedEvent(Role Role) : DomainEvent;
