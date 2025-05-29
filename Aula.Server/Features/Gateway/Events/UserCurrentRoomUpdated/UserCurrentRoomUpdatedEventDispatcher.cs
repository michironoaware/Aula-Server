using Aula.Server.Domain.Users;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.UserCurrentRoomUpdated;

internal sealed class UserCurrentRoomUpdatedEventDispatcher : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public UserCurrentRoomUpdatedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(UserCurrentRoomUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<UserCurrentRoomUpdatedEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserCurrentRoomUpdated,
			Data = new UserCurrentRoomUpdatedEventData
			{
				UserId = notification.UserId,
				PreviousRoomId = notification.PreviousRoomId,
				CurrentRoomId = notification.CurrentRoomId,
			},
		};

		await _gatewayManager.DispatchEventAsync(payload);
	}
}
