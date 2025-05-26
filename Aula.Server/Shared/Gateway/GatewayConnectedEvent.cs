using MediatR;

namespace Aula.Server.Shared.Gateway;

internal sealed class GatewayConnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }

	internal required PresenceOption Presence { get; init; }
}
