using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Messages;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using Aula.Server.Shared.Identity;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.MessageCreated;

internal sealed class MessageCreatedEventDispatcher : INotificationHandler<MessageCreatedEvent>
{
	private readonly GatewayManager _gatewayManager;
	private readonly UserManager _userManager;

	public MessageCreatedEventDispatcher(GatewayManager gatewayManager, UserManager userManager)
	{
		_gatewayManager = gatewayManager;
		_userManager = userManager;
	}

	public async Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<MessageData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.MessageCreated,
			Data = notification.Message.ToMessageData(),
		};

		await _gatewayManager.DispatchEventAsync(payload, async (user, intents, state)
				=> intents.HasFlag(Intents.Messages) &&
				(user.CurrentRoomId == state.Message.RoomId ||
					await state._userManager.HasPermissionAsync(user, Permissions.Administrator))
			, (_userManager, notification.Message), cancellationToken);
	}
}
