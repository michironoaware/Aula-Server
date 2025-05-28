using Aula.Server.Domain.Rooms;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.RoomRemoved;

internal sealed class RoomRemovedEventDispatcher : INotificationHandler<RoomDeletedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public RoomRemovedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(RoomDeletedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<RoomData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoomDeleted,
			Data = notification.Room.ToRoomData(),
		};

		await _gatewayManager.DispatchEventAsync(payload, Intents.Rooms);
	}
}
