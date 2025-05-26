using Aula.Server.Domain.Rooms;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.RoomCreated;

internal sealed class RoomCreatedEventDispatcher : INotificationHandler<RoomCreatedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public RoomCreatedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(RoomCreatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<RoomData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoomCreated,
			Data = notification.Room.ToRoomData(),
		};

		await _gatewayManager.DispatchEventAsync(payload, Intents.Rooms);
	}
}
