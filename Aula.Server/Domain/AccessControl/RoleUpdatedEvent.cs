namespace Aula.Server.Domain.AccessControl;

internal sealed record RoleUpdatedEvent(Role Role) : DomainEvent;
