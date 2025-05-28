namespace Aula.Server.Domain.Files;

internal sealed record FileDeletedEvent(File File) : DomainEvent;
