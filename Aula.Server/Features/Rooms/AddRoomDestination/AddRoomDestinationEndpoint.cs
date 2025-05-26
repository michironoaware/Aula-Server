using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Rooms;
using Aula.Server.Features.Rooms.Shared;
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

namespace Aula.Server.Features.Rooms.AddRoomDestination;

internal sealed class AddRoomDestinationEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("rooms/{roomId}/destinations/{destinationRoomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromRoute] Snowflake destinationRoomId,
		[FromServices] AppDbContext dbContext,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] IPublisher publisher,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		if (roomId == destinationRoomId)
			return TypedResults.Problem(ProblemDetailsDefaults.DestinationRoomCannotBeSourceRoom);

		var room = await dbContext.Rooms
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.Include(r => r.Destinations)
			.FirstOrDefaultAsync(ct);
		if (room is null)
			return TypedResults.NotFound();

		var destinationRoomExists = await dbContext.Rooms
			.Where(r => r.Id == destinationRoomId && !r.IsRemoved)
			.AnyAsync(ct);
		if (!destinationRoomExists)
			return TypedResults.Problem(ProblemDetailsDefaults.DestinationRoomDoesNotExist);

		var roomConnectionExists = room.Destinations
			.Any(r => r.SourceRoomId == roomId && r.DestinationRoomId == destinationRoomId);
		if (roomConnectionExists)
			return TypedResults.NoContent();

		room.Destinations.Add(new RoomConnection(await snowflakes.GenerateAsync(), roomId, destinationRoomId));
		room.ConcurrencyStamp = Guid.NewGuid().ToString("N");

		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new RoomUpdatedEvent(room), ct);

		return TypedResults.NoContent();
	}
}
