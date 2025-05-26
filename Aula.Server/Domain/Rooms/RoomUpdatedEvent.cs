namespace Aula.Server.Domain.Rooms;

internal sealed record RoomUpdatedEvent(Room Room) : DomainEvent;
