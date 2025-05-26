using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Bans;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Gateway;
using Aula.Server.Shared.Identity;
using MediatR;

namespace Aula.Server.Features.Gateway.Events.BanRemoved;

internal sealed class BanLiftedEventDispatcher : INotificationHandler<BanLiftedEvent>
{
	private readonly GatewayManager _gatewayManager;
	private readonly UserManager _userManager;

	public BanLiftedEventDispatcher(GatewayManager gatewayManager, UserManager userManager)
	{
		_gatewayManager = gatewayManager;
		_userManager = userManager;
	}

	public async Task Handle(BanLiftedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<BanData>
		{
			Operation = OperationType.Dispatch, Event = EventType.BanRemoved, Data = notification.Ban.ToBanData(),
		};

		await _gatewayManager.DispatchEventAsync(payload, async (user, intents, userManager)
				=> intents.HasFlag(Intents.Moderation) &&
				(await userManager.HasPermissionAsync(user, Permissions.BanUsers) ||
					await userManager.HasPermissionAsync(user, Permissions.Administrator))
			, _userManager, cancellationToken);
	}
}
