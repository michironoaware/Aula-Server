using System.Collections.Concurrent;
using System.Text.Json;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Json;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aula.Server.Shared.Gateway;

internal sealed class GatewayManager
{
	private readonly AppDbContext _dbContext;
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly IServiceScope _serviceScope;
	private readonly ConcurrentDictionary<String, GatewaySession> _sessions = [ ];

	public GatewayManager(
		IOptions<GatewayOptions> gatewayOptions,
		IOptions<JsonOptions> jsonOptions,
		AppDbContext dbContext,
		IServiceScopeFactory scopeFactory)
	{
		_dbContext = dbContext;
		_serviceScope = scopeFactory.CreateScope();
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		ExpirePeriod = TimeSpan.FromSeconds(gatewayOptions.Value.SecondsToExpire);
	}

	internal TimeSpan ExpirePeriod { get; }

	internal IReadOnlyDictionary<String, GatewaySession> Sessions => _sessions;

	internal GatewaySession CreateSession(
		Snowflake userId,
		Intents intents)
	{
		var session = new GatewaySession(userId,
			intents,
			_jsonSerializerOptions,
			_serviceScope.ServiceProvider.GetRequiredService<IPublisher>(),
			_serviceScope.ServiceProvider.GetRequiredService<AppDbContext>());

		_ = _sessions.TryAdd(session.Id, session);
		return session;
	}

	internal async Task DispatchEventAsync<TPayloadData>(
		GatewayPayload<TPayloadData> payload)
	{
		await DispatchEventAsync(payload, 0);
	}

	internal async Task DispatchEventAsync<TPayloadData>(
		GatewayPayload<TPayloadData> payload,
		Intents requiredIntents)
	{
		var sessions = _sessions.Select(kvp => kvp.Value);

		Byte[]? payloadBytes = null;
		foreach (var session in sessions)
		{
			if (!session.Intents.HasFlag(requiredIntents))
				continue;

			payloadBytes ??= payload.GetJsonUtf8Bytes(_jsonSerializerOptions);
			await session.QueueEventAsync(payloadBytes, CancellationToken.None);
		}
	}

	internal async Task DispatchEventAsync<TPayloadData>(
		GatewayPayload<TPayloadData> payload,
		Func<User, Intents, ValueTask<Boolean>> predicate,
		CancellationToken ct = default)
	{
		var sessionsByUserId = _sessions.Values.ToDictionary(s => s.UserId);
		var sessionUsers = await _dbContext.Users
			.AsNoTrackingWithIdentityResolution()
			.Where(u => sessionsByUserId.Keys.Contains(u.Id))
			.Include(u => u.RoleAssignments)
			.ThenInclude(ra => ra.Role)
			.ToArrayAsync(ct);

		Byte[]? payloadBytes = null;
		foreach (var user in sessionUsers)
		{
			var userSession = sessionsByUserId[user.Id];
			if (!await predicate.Invoke(user, userSession.Intents))
				continue;

			payloadBytes ??= payload.GetJsonUtf8Bytes(_jsonSerializerOptions);
			await userSession.QueueEventAsync(payloadBytes, CancellationToken.None);
		}
	}

	internal async Task DispatchEventAsync<TPayloadData, TState>(
		GatewayPayload<TPayloadData> payload,
		Func<User, Intents, TState, ValueTask<Boolean>> predicate,
		TState state,
		CancellationToken ct = default)
	{
		var sessionsByUserId = _sessions.Values.ToDictionary(s => s.UserId);
		var sessionUsers = await _dbContext.Users
			.AsNoTrackingWithIdentityResolution()
			.Where(u => sessionsByUserId.Keys.Contains(u.Id))
			.Include(u => u.RoleAssignments)
			.ThenInclude(ra => ra.Role)
			.ToArrayAsync(ct);

		Byte[]? payloadBytes = null;
		foreach (var user in sessionUsers)
		{
			var userSession = sessionsByUserId[user.Id];
			if (!await predicate.Invoke(user, userSession.Intents, state))
				continue;

			payloadBytes ??= payload.GetJsonUtf8Bytes(_jsonSerializerOptions);
			await userSession.QueueEventAsync(payloadBytes, CancellationToken.None);
		}
	}

	internal Int32 ClearExpiredSessions()
	{
		var expiredSessions = _sessions.Values
			.Where(s => s.CloseDate < DateTime.UtcNow - ExpirePeriod)
			.ToArray();
		foreach (var session in expiredSessions)
			_ = _sessions.TryRemove(session.Id, out _);
		return expiredSessions.Length;
	}
}
