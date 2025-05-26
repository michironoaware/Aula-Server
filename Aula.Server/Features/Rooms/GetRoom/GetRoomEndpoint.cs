using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Rooms.GetRoom;

internal sealed class GetRoomEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<RoomData>, NotFound>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromServices] AppDbContext dbContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var room = await dbContext.Rooms
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.Select(r =>
				new RoomData
				{
					Id = r.Id,
					Type = r.Type,
					Name = r.Name,
					Description = r.Description,
					IsEntrance = r.IsEntrance,
					BackgroundAudioId = r.BackgroundAudioId,
					DestinationIds = r.Destinations.Select(rc => rc.DestinationRoomId),
					ResidentIds = r.Residents.Select(u => u.Id),
					CreationDate = r.CreationDate,
				})
			.FirstOrDefaultAsync(ct);
		if (room is null)
			return TypedResults.NotFound();

		return TypedResults.Ok(room);
	}
}
