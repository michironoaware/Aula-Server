namespace Aula.Server.Domain.Rooms;

internal sealed record RoomRemovedEvent(Room Room) : DomainEvent;
