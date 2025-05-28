using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Rooms;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Rooms.DeleteRoom;

internal sealed class DeleteRoom : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{roomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromServices] AppDbContext dbContext,
		[FromServices] IPublisher publisher,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var room = await dbContext.Rooms
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.FirstOrDefaultAsync(ct);
		if (room is null)
			return TypedResults.NoContent();

		room.Origins.Clear();
		room.Destinations.Clear();
		room.IsRemoved = true;
		room.ConcurrencyStamp = Guid.NewGuid().ToString("N");

		var usersInRoom = await dbContext.Users
			.Where(user => user.CurrentRoomId == roomId)
			.ToArrayAsync(ct);
		foreach (var u in usersInRoom)
		{
			u.CurrentRoomId = null;
			u.ConcurrencyStamp = Guid.NewGuid().ToString("N");
		}

		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new RoomDeletedEvent(room), CancellationToken.None);
		return TypedResults.NoContent();
	}
}
