using System.Net.WebSockets;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Gateway;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Resilience;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Aula.Server.Features.Bans;

internal sealed class BannedUserSessionTerminator : INotificationHandler<BanIssuedEvent>
{
	private readonly AppDbContext _dbContext;
	private readonly GatewayManager _gatewayManager;
	private readonly IPublisher _publisher;
	private readonly ResiliencePipeline _retryOnDbConcurrencyProblem;

	public BannedUserSessionTerminator(
		AppDbContext dbContext,
		GatewayManager gatewayManager,
		IPublisher publisher,
		[FromKeyedServices(ResiliencePipelines.RetryOnDbConcurrencyProblem)]
		ResiliencePipeline retryOnDbConcurrencyProblem)
	{
		_dbContext = dbContext;
		_gatewayManager = gatewayManager;
		_publisher = publisher;
		_retryOnDbConcurrencyProblem = retryOnDbConcurrencyProblem;
	}

	public async Task Handle(BanIssuedEvent notification, CancellationToken cancellationToken)
	{
		if (notification.Ban is not UserBan ban)
			return;

		var targetSessions = _gatewayManager.Sessions
			.Select(kvp => kvp.Value)
			.Where(s => s.UserId == ban.TargetUserId);
		foreach (var session in targetSessions)
			await session.StopAsync(WebSocketCloseStatus.NormalClosure);

		await _retryOnDbConcurrencyProblem.ExecuteAsync(static async (state, ct) =>
		{
			var user = await state.DbContext.Users
				.Where(u => u.Id == state.Ban.TargetUserId)
				.FirstOrDefaultAsync(ct);
			if (user is null)
				return;

			user.RoleAssignments.Clear();
			user.CurrentRoomId = null;
			user.ConcurrencyStamp = Guid.NewGuid().ToString("N");
			_ = await state.DbContext.SaveChangesAsync(ct);
			await state.Publisher.Publish(new UserUpdatedEvent(user), CancellationToken.None);
		}, (DbContext: _dbContext, Publisher: _publisher, Ban: ban), cancellationToken);
	}
}
