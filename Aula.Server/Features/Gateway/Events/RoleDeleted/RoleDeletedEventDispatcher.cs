using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.RoleDeleted;

internal sealed class RoleDeletedEventDispatcher : INotificationHandler<RoleDeletedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public RoleDeletedEventDispatcher(GatewayManager gatewayManager)
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
