using Aula.Server.Features.Rooms.Shared;
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

namespace Aula.Server.Features.Rooms.GetRooms;

internal sealed class GetRoomsEndpoint : IApiEndpoint
{
	internal const String CountQueryParamName = "count";
	internal const String AfterQueryParamName = "after";
	internal const Int32 MinimumRoomCount = 1;
	internal const Int32 MaximumRoomCount = 100;
	internal const Int32 DefaultRoomCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<RoomData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = CountQueryParamName)] Int32? specifiedCount,
		[FromQuery(Name = AfterQueryParamName)] Snowflake? afterRoomId,
		[FromServices] AppDbContext dbContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var count = specifiedCount ?? DefaultRoomCount;
		if (count is > MaximumRoomCount or < MinimumRoomCount)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidRoomCount);

		var roomsQuery = dbContext.Rooms
			.Where(r => !r.IsRemoved)
			.OrderBy(r => r.CreationDate)
			.AsQueryable();

		if (afterRoomId is not null)
		{
			var targetRoom = await dbContext.Rooms
				.Where(r => r.Id == afterRoomId && !r.IsRemoved)
				.Select(r => new { r.CreationDate })
				.FirstOrDefaultAsync(ct);
			if (targetRoom is null)
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterRoomQueryParam);

			roomsQuery = roomsQuery.Where(r => r.CreationDate > targetRoom.CreationDate);
		}

		var rooms = await roomsQuery
			.Select(r => new RoomData
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
			.Take(count)
			.ToArrayAsync(ct);
		return TypedResults.Ok(rooms.AsEnumerable());
	}
}
