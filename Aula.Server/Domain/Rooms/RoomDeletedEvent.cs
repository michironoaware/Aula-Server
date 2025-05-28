namespace Aula.Server.Domain.Rooms;

internal sealed record RoomDeletedEvent(Room Room) : DomainEvent;
