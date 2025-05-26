using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Gateway;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Features.Gateway.ConnectToGateway;

internal sealed class ConnectToGatewayEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.Map("gateway", HandleAsync)
			.ApplyRateLimiting(GatewayRateLimitingPolicy.Name)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<EmptyHttpResult, ProblemHttpResult, InternalServerError>> HandleAsync(
		HttpContext httpContext,
		[FromHeader(Name = "X-Intents")] Intents intents,
		[FromHeader(Name = "X-SessionId")] String? sessionId,
		[FromHeader(Name = "X-Presence")] PresenceOption? presence,
		[FromServices] UserManager userManager,
		[FromServices] GatewayManager gatewayManager)
	{
		if (!httpContext.WebSockets.IsWebSocketRequest)
			return TypedResults.Problem(ProblemDetailsDefaults.NotAGatewayRequest);

		var currentUser = await userManager.GetUserAsync(httpContext.User);
		if (currentUser is null)
			return TypedResults.InternalServerError();

		if (presence is not null &&
		    !await userManager.HasPermissionAsync(currentUser, Permissions.Administrator))
			return TypedResults.Problem(ProblemDetailsDefaults.OnlyAdminCanProvidePresence);

		GatewaySession session;

		if (sessionId is not null)
		{
			if (!gatewayManager.Sessions.TryGetValue(sessionId, out var previousSession) ||
			    previousSession.UserId != currentUser.Id ||
			    previousSession.IsActive ||
			    previousSession.CloseDate < DateTime.UtcNow - gatewayManager.ExpirePeriod)
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidSession);

			var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
			previousSession.SetWebSocket(socket);

			session = previousSession;
		}
		else
		{
			var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
			session = gatewayManager.CreateSession(currentUser.Id, intents);
			session.SetWebSocket(socket);
		}

		await session.RunAsync(presence ?? PresenceOption.Online);

		return TypedResults.Empty;
	}
}
