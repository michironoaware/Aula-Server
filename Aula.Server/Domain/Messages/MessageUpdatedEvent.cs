namespace Aula.Server.Domain.Messages;

internal sealed record MessageUpdatedEvent(Message Message) : DomainEvent;
