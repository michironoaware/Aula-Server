using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.RoleCreated;

internal sealed class RoleCreatedEventDispatcher : INotificationHandler<RoleCreatedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public RoleCreatedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(RoleCreatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<RoleData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoleCreated,
			Data = notification.Role.ToRoleData(),
		};

		await _gatewayManager.DispatchEventAsync(payload);
	}
}
