using Aula.Server.Domain.AccessControl;
using Aula.Server.Features.Messages.Shared;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Messages.GetMessage;

internal sealed class GetMessageEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}/messages/{messageId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ReadMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<MessageData>, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromRoute] Snowflake messageId,
		[FromServices] AppDbContext dbContext,
		[FromServices] UserManager userManager,
		HttpContext httpContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var roomExists = await dbContext.Rooms
			.Where(room => room.Id == roomId && !room.IsRemoved)
			.AnyAsync(ct);
		if (!roomExists)
			return TypedResults.NotFound();

		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
			return TypedResults.InternalServerError();
		if (user.CurrentRoomId != roomId &&
		    !await userManager.HasPermissionAsync(user, Permissions.Administrator))
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsNotInTheRoom);

		var message = await dbContext.Messages
			.AsNoTracking()
			.Where(m => m.Id == messageId && m.RoomId == roomId && !m.IsRemoved)
			.FirstOrDefaultAsync(ct);
		if (message is null)
			return TypedResults.NotFound();

		return TypedResults.Ok(message.ToMessageData());
	}
}
