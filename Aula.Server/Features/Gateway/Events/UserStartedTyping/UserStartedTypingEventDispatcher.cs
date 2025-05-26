using Aula.Server.Domain.AccessControl;
using Aula.Server.Features.Messages.StartTyping;
using Aula.Server.Shared.Gateway;
using Aula.Server.Shared.Identity;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.UserStartedTyping;

internal sealed class UserStartedTypingEventDispatcher : INotificationHandler<UserStartedTypingEvent>
{
	private readonly GatewayManager _gatewayManager;
	private readonly UserManager _userManager;

	public UserStartedTypingEventDispatcher(GatewayManager gatewayManager, UserManager userManager)
	{
		_gatewayManager = gatewayManager;
		_userManager = userManager;
	}

	public async Task Handle(UserStartedTypingEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<UserStartedTypingEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserStartedTyping,
			Data = new UserStartedTypingEventData { UserId = notification.UserId, RoomId = notification.RoomId },
		};

		await _gatewayManager.DispatchEventAsync(payload, async (user, intents, state)
				=> intents.HasFlag(Intents.Messages) &&
				(user.CurrentRoomId == state.RoomId ||
					await state._userManager.HasPermissionAsync(user, Permissions.Administrator))
			, (_userManager, notification.RoomId), cancellationToken);
	}
}
