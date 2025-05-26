namespace Aula.Server.Domain.Messages;

internal sealed record MessageRemovedEvent(Message Message) : DomainEvent;
