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

namespace Aula.Server.Features.Rooms.CreateRoom;

internal sealed class CreateRoomEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("rooms", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Created<RoomData>, ProblemHttpResult>> HandleAsync(
		[FromBody] CreateRoomRequestBody body,
		[FromServices] IValidator<CreateRoomRequestBody> bodyValidator,
		[FromServices] AppDbContext dbContext,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] IPublisher publisher,
		HttpContext httpContext,
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

		var id = await snowflakes.GenerateAsync();
		var name = body.Name;
		var description = body.Description ?? String.Empty;
		var isEntrance = body.IsEntrance ?? false;
		var room = new StandardRoom(id, name, description, isEntrance, body.BackgroundAudioId);

		_ = dbContext.Rooms.Add(room);
		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new RoomCreatedEvent(room), ct);

		return TypedResults.Created($"{httpContext.Request.GetUrl()}/{room.Id}", new RoomData
		{
			Id = room.Id,
			Type = room.Type,
			Name = room.Name,
			Description = room.Description,
			IsEntrance = room.IsEntrance,
			BackgroundAudioId = room.BackgroundAudioId,
			ResidentIds = [ ],
			DestinationIds = [ ],
			CreationDate = room.CreationDate,
		});
	}
}
