namespace Aula.Server.Domain.Files;

internal sealed record FileRemovedEvent(File File) : DomainEvent;
