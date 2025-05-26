namespace Aula.Server.Domain.Files;

internal sealed record FileCreatedEvent(File File) : DomainEvent;
