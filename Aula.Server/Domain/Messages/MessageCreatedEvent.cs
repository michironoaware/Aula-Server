namespace Aula.Server.Domain.Messages;

internal sealed record MessageCreatedEvent(Message Message) : DomainEvent;
