using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Rooms;
using Aula.Server.Features.Rooms.Shared;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Rooms.ModifyRoom;

internal sealed class ModifyRoomEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPatch("rooms/{roomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<RoomData>, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromBody] ModifyRoomRequestBody body,
		[FromServices] IValidator<ModifyRoomRequestBody> bodyValidator,
		[FromServices] AppDbContext dbContext,
		[FromServices] IPublisher publisher,
		[FromServices] ISnowflakeGenerator snowflakes,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var validation = await bodyValidator.ValidateAsync(body, ct);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var backgroundAudioFileExists = await dbContext.Files
			.Where(f => f.Id == body.BackgroundAudioId && !f.IsRemoved)
			.AnyAsync(ct);
		if (!backgroundAudioFileExists)
			return TypedResults.Problem(ProblemDetailsDefaults.BackgroundAudioFileDoesNotExist);

		var room = await dbContext.Rooms
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.Include(r => r.Residents)
			.Include(r => r.Destinations)
			.FirstOrDefaultAsync(ct);
		if (room is null)
			return TypedResults.NotFound();

		var modified = false;
		if (body.Name is not null)
		{
			room.Name = body.Name;
			modified = true;
		}

		if (body.Description is not null)
		{
			room.Description = body.Description;
			modified = true;
		}

		if (body.IsEntrance is not null)
		{
			room.IsEntrance = (Boolean)body.IsEntrance;
			modified = true;
		}

		if (body.BackgroundAudioId is not null)
		{
			room.BackgroundAudioId = body.BackgroundAudioId;
			modified = true;
		}

		if (body.DestinationIds is not null)
		{
			var destinationIds = body.DestinationIds.Distinct().ToArray();
			var targetRoomCount = await dbContext.Rooms
				.Where(r => destinationIds.Contains(r.Id) && !r.IsRemoved)
				.CountAsync(ct);
			if (targetRoomCount < destinationIds.Length)
				return TypedResults.Problem(ProblemDetailsDefaults.OneDestinationRoomDoesNotExist);

			var alreadyConnectedDestinationIds = room.Destinations
				.Where(c => destinationIds.Contains(c.DestinationRoomId))
				.Select(c => c.DestinationRoomId)
				.ToArray();

			var newConnections = body.DestinationIds
				.Except(alreadyConnectedDestinationIds)
				.Select(targetId => new RoomConnection(snowflakes.Generate(), roomId, targetId))
				.ToArray();

			_ = room.Destinations.RemoveAll(c => !destinationIds.Contains(c.DestinationRoomId));
			room.Destinations.AddRange(newConnections);

			if (alreadyConnectedDestinationIds.Length != room.Destinations.Count ||
			    newConnections.Length is not 0)
				modified = true;
		}

		if (modified)
		{
			room.ConcurrencyStamp = Guid.NewGuid().ToString("N");
			_ = await dbContext.SaveChangesAsync(ct);
			await publisher.Publish(new RoomUpdatedEvent(room), CancellationToken.None);
		}

		return TypedResults.Ok(new RoomData
		{
			Id = room.Id,
			Type = room.Type,
			Name = room.Name,
			Description = room.Description,
			IsEntrance = room.IsEntrance,
			BackgroundAudioId = room.BackgroundAudioId,
			ResidentIds = room.Residents.Select(u => u.Id),
			DestinationIds = room.Destinations.Select(rc => rc.DestinationRoomId),
			CreationDate = room.CreationDate,
		});
	}
}
