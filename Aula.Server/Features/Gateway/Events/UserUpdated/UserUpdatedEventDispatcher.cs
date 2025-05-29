using Aula.Server.Domain.Users;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.UserUpdated;

internal sealed class UserUpdatedEventDispatcher : INotificationHandler<UserUpdatedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public UserUpdatedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<UserData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserUpdated,
			Data = notification.User.ToUserData(),
		};

		await _gatewayManager.DispatchEventAsync(payload);
	}
}
