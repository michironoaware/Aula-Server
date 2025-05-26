using MediatR;

namespace Aula.Server.Domain;

internal abstract record DomainEvent : INotification;
