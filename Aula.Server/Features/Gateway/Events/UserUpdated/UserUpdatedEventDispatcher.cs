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

	public Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var user = notification.User;
		var payload = new GatewayPayload<UserData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserUpdated,
			Data = new UserData
			{
				Id = user.Id,
				DisplayName = user.DisplayName,
				Description = user.Description,
				Type = user.Type,
				Presence = user.Presence,
				RoleIds = user.RoleAssignments.Select(ra => ra.Role.Id),
				CurrentRoomId = user.CurrentRoomId,
			},
		};

		foreach (var session in _gatewayManager.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Users))
				continue;

			_ = session.QueueEventAsync(payload, cancellationToken);
		}

		return Task.CompletedTask;
	}
}
