namespace Aula.Server.Domain.Files;

internal sealed record FileUpdatedEvent(File File) : DomainEvent;
