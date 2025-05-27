using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;
using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Users;
using Aula.Server.Features.Gateway.Events.UpdatePresence;
using Aula.Server.Shared.Gateway;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Resilience;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;

namespace Aula.Server.Features.Presences;

internal sealed class PresenceUpdater :
	INotificationHandler<GatewayConnectedEvent>,
	INotificationHandler<PayloadReceivedEvent>,
	INotificationHandler<GatewayDisconnectedEvent>
{
	private static readonly ConcurrentDictionary<Snowflake, UserPresenceState> s_presenceStates = new();
	private readonly AppDbContext _dbContext;
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly IPublisher _publisher;
	private readonly ResiliencePipeline _retryOnDbConcurrencyProblem;
	private readonly UserManager _userManager;

	public PresenceUpdater(
		IOptions<JsonOptions> jsonOptions,
		AppDbContext dbContext,
		IPublisher publisher,
		UserManager userManager,
		[FromKeyedServices(ResiliencePipelines.RetryOnDbConcurrencyProblem)]
		ResiliencePipeline retryOnDbConcurrencyProblem)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_dbContext = dbContext;
		_publisher = publisher;
		_userManager = userManager;
		_retryOnDbConcurrencyProblem = retryOnDbConcurrencyProblem;
	}

	public async Task Handle(GatewayConnectedEvent notification, CancellationToken cancellationToken)
	{
		var session = notification.Session;
		var presenceState = s_presenceStates.GetOrAdd(session.UserId, _ => new UserPresenceState());

		await presenceState.UpdateSemaphore.WaitAsync(cancellationToken);

		presenceState.GatewayCount++;

		await _retryOnDbConcurrencyProblem.ExecuteAsync(static async (state, ct) =>
			{
				var user = await state.DbContext.Users
					.AsTracking()
					.Where(u => u.Id == state.Session.UserId)
					.FirstOrDefaultAsync(ct);

				user!.Presence = GetPresenceFromOption(state.Notification.Presence);
				user.ConcurrencyStamp = Guid.NewGuid().ToString("N");
				_ = await state.DbContext.SaveChangesAsync(ct);
				await state.Publisher.Publish(new UserPresenceUpdatedEvent(user.Id, user.Presence),
					CancellationToken.None);
				_ = state.PresenceState.UpdateSemaphore.Release();
			},
			(DbContext: _dbContext, PresenceState: presenceState, Session: session, Publisher: _publisher,
				Notification: notification), cancellationToken);
	}

	public async Task Handle(GatewayDisconnectedEvent notification, CancellationToken cancellationToken)
	{
		var session = notification.Session;
		if (!s_presenceStates.TryGetValue(session.UserId, out var presenceState))
			throw new UnreachableException("Expected gateway state to be traced.");

		await presenceState.UpdateSemaphore.WaitAsync(cancellationToken);

		if (presenceState.GatewayCount > 1)
			presenceState.GatewayCount--;
		else
			_ = s_presenceStates.TryRemove(session.UserId, out _);

		await _retryOnDbConcurrencyProblem.ExecuteAsync(static async (state, ct) =>
			{
				var user = await state.DbContext.Users
					.AsTracking()
					.Where(u => u.Id == state.Session.UserId)
					.FirstOrDefaultAsync(ct);

				user!.Presence = Presence.Offline;
				user.ConcurrencyStamp = Guid.NewGuid().ToString("N");
				_ = await state.DbContext.SaveChangesAsync(ct);
				await state.Publisher.Publish(new UserPresenceUpdatedEvent(user.Id, user.Presence),
					CancellationToken.None);
				_ = state.PresenceState.UpdateSemaphore.Release();
			}, (DbContext: _dbContext, PresenceState: presenceState, Session: session, Publisher: _publisher),
			cancellationToken);
	}

	public async Task Handle(PayloadReceivedEvent notification, CancellationToken cancellationToken)
	{
		var payload = notification.Payload;
		if (payload.Operation is not OperationType.Dispatch ||
		    payload.Event is not EventType.UpdatePresence)
			return;

		var session = notification.Session;

		var user = await _dbContext.Users
			.Where(u => u.Id == session.UserId)
			.Select(u => new { Permissions = u.RoleAssignments.Select(ra => ra.Role.Permissions ) })
			.FirstOrDefaultAsync(cancellationToken);

		if (user is null || !await _userManager.HasPermissionAsync(user.Permissions, Permissions.Administrator))
			return;

		UpdatePresenceEventData data;
		try
		{
			data = payload.Data.Deserialize<UpdatePresenceEventData>(_jsonSerializerOptions) ??
				throw new JsonException("Data expected to not be null");
		}
		catch (JsonException)
		{
			await session.StopAsync(WebSocketCloseStatus.InvalidPayloadData);
			return;
		}

		if (!s_presenceStates.TryGetValue(session.UserId, out var presenceState))
			throw new UnreachableException("Expected gateway state to be traced.");

		await presenceState.UpdateSemaphore.WaitAsync(cancellationToken);
		await _retryOnDbConcurrencyProblem.ExecuteAsync(static async (state, ct) =>
			{
				var user = await state.DbContext.Users
					.AsTracking()
					.Where(u => u.Id == state.Session.UserId)
					.FirstOrDefaultAsync(ct);

				user!.Presence = GetPresenceFromOption(state.Presence);
				user.ConcurrencyStamp = Guid.NewGuid().ToString("N");
				_ = await state.DbContext.SaveChangesAsync(ct);
				await state.Publisher.Publish(new UserPresenceUpdatedEvent(user.Id, user.Presence),
					CancellationToken.None);
				_ = state.PresenceState.UpdateSemaphore.Release();
			},
			(DbContext: _dbContext, PresenceState: presenceState, Session: session, Publisher: _publisher,
				data.Presence),
			cancellationToken);
	}

	private static Presence GetPresenceFromOption(PresenceOption presenceOption)
	{
		return presenceOption switch
		{
			PresenceOption.Invisible => Presence.Offline,
			PresenceOption.Online => Presence.Online,
			_ => throw new UnreachableException($"Unhandled {nameof(PresenceOption)} case: {presenceOption})"),
		};
	}

	private sealed class UserPresenceState
	{
		internal Int32 GatewayCount { get; set; }

		internal SemaphoreSlim UpdateSemaphore { get; } = new(1, 1);
	}
}
