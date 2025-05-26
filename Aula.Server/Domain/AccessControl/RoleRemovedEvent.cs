namespace Aula.Server.Domain.AccessControl;

internal sealed record RoleRemovedEvent(Role Role) : DomainEvent;
