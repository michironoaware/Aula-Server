using Aula.Server.Domain.Users;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.UserPresenceUpdated;

internal sealed class UserPresenceUpdatedEventDispatcher : INotificationHandler<UserPresenceUpdatedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public UserPresenceUpdatedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(UserPresenceUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<UserPresenceUpdatedEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserPresenceUpdated,
			Data = new UserPresenceUpdatedEventData
			{
				UserId = notification.UserId, Presence = notification.Presence,
			},
		};

		await _gatewayManager.DispatchEventAsync(payload);
	}
}
