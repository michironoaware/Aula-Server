using System.Net.WebSockets;
using Aula.Server.Domain.Users;
using MediatR;

namespace Aula.Server.Shared.Gateway;

internal sealed class UserSessionTerminatorService : INotificationHandler<UserDeletedEvent>,
	INotificationHandler<UserSecurityStampUpdatedEvent>
{
	private readonly GatewayManager _gatewayManager;

	public UserSessionTerminatorService(GatewayManager gatewayManager)
	{
		_gatewayManager = gatewayManager;
	}

	public Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
	{
		var userSessions = _gatewayManager.Sessions
			.Select(kvp => kvp.Value)
			.Where(s => s.UserId == notification.User.Id);

		foreach (var session in userSessions)
			_ = session.StopAsync(WebSocketCloseStatus.NormalClosure);

		return Task.CompletedTask;
	}

	public Task Handle(UserSecurityStampUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var userSessions = _gatewayManager.Sessions
			.Select(kvp => kvp.Value)
			.Where(s => s.UserId == notification.UserId);

		foreach (var session in userSessions)
			_ = session.StopAsync(WebSocketCloseStatus.NormalClosure);

		return Task.CompletedTask;
	}
}
