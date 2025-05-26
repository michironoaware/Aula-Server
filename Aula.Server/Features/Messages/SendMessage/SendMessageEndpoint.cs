using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Messages;
using Aula.Server.Domain.Rooms;
using Aula.Server.Features.Messages.Shared;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.RateLimiting;
using Aula.Server.Shared.Snowflakes;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Messages.SendMessage;

internal sealed class SendMessageEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("rooms/{roomId}/messages", HandleAsync)
			.ApplyRateLimiting(SendMessageRateLimitingPolicy.Name)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.SendMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Created<MessageData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromBody] SendMessageRequestBody body,
		[FromServices] IValidator<SendMessageRequestBody> bodyValidator,
		HttpContext httpContext,
		[FromServices] UserManager userManager,
		[FromServices] AppDbContext dbContext,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] IPublisher publisher,
		CancellationToken ct)
	{
		var validation = await bodyValidator.ValidateAsync(body, ct);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var room = await dbContext.Rooms
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.Select(r => new { r.Type })
			.FirstOrDefaultAsync(ct);
		if (room is null)
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		if (room.Type is not RoomType.Standard)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidRoomType);

		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
			return TypedResults.InternalServerError();
		if (user.CurrentRoomId != roomId)
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsNotInTheRoom);

		if (body.Type is not MessageType.Default)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidMessageType);

		var messageId = await snowflakes.GenerateAsync();
		var flags = body.Flags ?? 0;
		var message = new DefaultMessage(messageId, flags, user.Id, roomId, body.Text!);

		_ = dbContext.Messages.Add(message);
		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new MessageCreatedEvent(message), CancellationToken.None);

		return TypedResults.Created($"{httpContext.Request.GetUrl()}/{messageId}", message.ToMessageData());
	}
}
