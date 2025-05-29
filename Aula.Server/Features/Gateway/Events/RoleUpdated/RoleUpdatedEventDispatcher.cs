using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.RoleUpdated;

internal sealed class RoleUpdatedEventDispatcher : INotificationHandler<RoleUpdatedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public RoleUpdatedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(RoleUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<RoleData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoleUpdated,
			Data = notification.Role.ToRoleData(),
		};

		await _gatewayManager.DispatchEventAsync(payload);
	}
}
