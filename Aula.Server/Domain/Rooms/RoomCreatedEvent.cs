namespace Aula.Server.Domain.Rooms;

internal sealed record RoomCreatedEvent(Room Room) : DomainEvent;
