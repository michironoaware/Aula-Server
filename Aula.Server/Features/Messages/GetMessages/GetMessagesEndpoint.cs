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

namespace Aula.Server.Features.Messages.GetMessages;

internal sealed class GetMessagesEndpoint : IApiEndpoint
{
	internal const String BeforeQueryParameter = "before";
	internal const String AfterQueryParameter = "after";
	internal const String CountQueryParameter = "count";
	internal const Int32 MinimumMessageCount = 1;
	internal const Int32 MaximumMessageCount = 100;
	internal const Int32 DefaultMessageCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}/messages", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ReadMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<MessageData>>, NotFound, ProblemHttpResult, InternalServerError>>
		HandleAsync(
			[FromRoute] Snowflake roomId,
			[FromQuery(Name = BeforeQueryParameter)] Snowflake? beforeMessageId,
			[FromQuery(Name = AfterQueryParameter)] Snowflake? afterMessageId,
			[FromQuery(Name = CountQueryParameter)] Int32? specifiedCount,
			[FromServices] AppDbContext dbContext,
			[FromServices] UserManager userManager,
			HttpContext httpContext,
			CancellationToken ct)
	{
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
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsNotInTheRoomAndNoAdministrator);

		var count = specifiedCount ?? DefaultMessageCount;
		if (count is > MaximumMessageCount or < MinimumMessageCount)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidMessageCount);

		var messagesQuery = dbContext.Messages
			.Where(m => !m.IsRemoved && m.RoomId == roomId)
			.OrderByDescending(m => m.CreationDate)
			.AsQueryable();

		if (beforeMessageId is not null)
		{
			var targetMessage = await dbContext.Messages
				.Where(m => m.Id == beforeMessageId && !m.IsRemoved && m.RoomId == roomId)
				.Select(m => new { m.CreationDate })
				.FirstOrDefaultAsync(ct);

			if (targetMessage is null)
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidBeforeMessage);

			messagesQuery = messagesQuery.Where(m => m.CreationDate < targetMessage.CreationDate);
		}
		else if (afterMessageId is not null)
		{
			var targetMessage = await dbContext.Messages
				.Where(m => m.Id == afterMessageId && !m.IsRemoved && m.RoomId == roomId)
				.Select(m => new { m.CreationDate })
				.FirstOrDefaultAsync(ct);

			if (targetMessage is null)
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterMessage);

			messagesQuery = messagesQuery.Where(m => m.CreationDate > targetMessage.CreationDate);
		}

		var messages = await messagesQuery
			.AsNoTracking()
			.Take(count)
			.ToArrayAsync(ct);
		return TypedResults.Ok(messages.Select(m => m.ToMessageData()));
	}
}
