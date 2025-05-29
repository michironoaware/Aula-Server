using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.RoleRemoved;

internal sealed class RoleRemovedEventDispatcher : INotificationHandler<RoleDeletedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public RoleRemovedEventDispatcher(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public async Task Handle(RoleDeletedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<RoleData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoleDeleted,
			Data = notification.Role.ToRoleData(),
		};

		await _gatewayManager.DispatchEventAsync(payload);
	}
}
