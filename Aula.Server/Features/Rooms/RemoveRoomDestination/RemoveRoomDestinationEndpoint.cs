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

namespace Aula.Server.Features.Rooms.RemoveRoomDestination;

internal sealed class RemoveRoomDestinationEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{sourceRoomId}/destinations/{targetRoomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> HandleAsync(
		[FromRoute] Snowflake sourceRoomId,
		[FromRoute] Snowflake targetRoomId,
		[FromServices] AppDbContext dbContext,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] IPublisher publisher,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var room = await dbContext.Rooms
			.Where(r => r.Id == sourceRoomId && !r.IsRemoved)
			.Include(r => r.Destinations)
			.FirstOrDefaultAsync(ct);
		if (room is null)
			return TypedResults.NoContent();

		var connection = room.Destinations
			.Find(c => c.SourceRoomId == sourceRoomId && c.DestinationRoomId == targetRoomId);
		if (connection is null ||
		    !room.Destinations.Remove(connection))
			return TypedResults.NoContent();

		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new RoomUpdatedEvent(room), CancellationToken.None);

		return TypedResults.NoContent();
	}
}
