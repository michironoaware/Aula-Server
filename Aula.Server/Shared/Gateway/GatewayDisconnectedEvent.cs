using MediatR;

namespace Aula.Server.Shared.Gateway;

internal sealed class GatewayDisconnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }
}
