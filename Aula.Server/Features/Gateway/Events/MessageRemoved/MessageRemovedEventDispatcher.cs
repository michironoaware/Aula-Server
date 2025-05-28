using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Messages;
using Aula.Server.Shared.Gateway;
using Aula.Server.Shared.Identity;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.MessageRemoved;

internal sealed class MessageRemovedEventDispatcher : INotificationHandler<MessageDeletedEvent>
{
	private readonly GatewayManager _gatewayManager;
	private readonly UserManager _userManager;

	public MessageRemovedEventDispatcher(GatewayManager gatewayManager, UserManager userManager)
	{
		_gatewayManager = gatewayManager;
		_userManager = userManager;
	}

	public async Task Handle(MessageDeletedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<MessageRemovedEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.MessageDeleted,
			Data = new MessageRemovedEventData { Id = notification.Message.Id },
		};

		await _gatewayManager.DispatchEventAsync(payload, async (user, intents, state)
				=> intents.HasFlag(Intents.Messages) &&
				(user.CurrentRoomId == state.Message.RoomId ||
					await state._userManager.HasPermissionAsync(user, Permissions.Administrator))
			, (_userManager, notification.Message), cancellationToken);
	}
}
