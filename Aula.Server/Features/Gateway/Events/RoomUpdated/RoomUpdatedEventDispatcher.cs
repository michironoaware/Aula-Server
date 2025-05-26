using Aula.Server.Domain.Rooms;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.RoomUpdated;

internal sealed class RoomUpdatedEventDispatcher : INotificationHandler<RoomUpdatedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public RoomUpdatedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(RoomUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<RoomData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoomUpdated,
			Data = notification.Room.ToRoomData(),
		};

		await _gatewayManager.DispatchEventAsync(payload, Intents.Rooms);
	}
}
