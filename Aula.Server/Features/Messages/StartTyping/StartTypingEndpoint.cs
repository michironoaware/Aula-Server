using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Rooms;
using Aula.Server.Features.Messages.Shared;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Messages.StartTyping;

internal sealed class StartTypingEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("rooms/{roomId}/start-typing", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.SendMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromServices] UserManager userManager,
		[FromServices] AppDbContext dbContext,
		[FromServices] IPublisher publisher,
		HttpContext httpContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var userId = userManager.GetUserId(httpContext.User);
		if (userId is null)
			return TypedResults.InternalServerError();

		var room = await dbContext.Rooms
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.Select(r => new { r.Type })
			.FirstOrDefaultAsync(ct);
		if (room is null)
			return TypedResults.NotFound();

		if (room.Type != RoomType.Standard)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidRoomType);

		await publisher.Publish(new UserStartedTypingEvent { UserId = (Snowflake)userId, RoomId = roomId },
			CancellationToken.None);

		return TypedResults.NoContent();
	}
}
