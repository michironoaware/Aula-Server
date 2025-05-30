using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Messages;
using Aula.Server.Features.Messages.Shared;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.RateLimiting;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Messages.DeleteMessage;

internal sealed class DeleteMessageEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{roomId}/messages/{messageId}", HandleAsync)
			.ApplyRateLimiting(DeleteMessageRateLimitingPolicy.Name)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromRoute] Snowflake messageId,
		[FromServices] AppDbContext dbContext,
		[FromServices] UserManager userManager,
		[FromServices] IPublisher publisher,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var roomExists = await dbContext.Rooms
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.AnyAsync(ct);
		if (!roomExists)
			return TypedResults.NotFound();

		var message = await dbContext.Messages
			.Where(m => m.Id == messageId && !m.IsRemoved)
			.FirstOrDefaultAsync(ct);
		if (message is null)
			return TypedResults.NoContent();

		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
			return TypedResults.InternalServerError();

		if (message.AuthorId != user.Id &&
		    !(await userManager.HasPermissionAsync(user, Permissions.Administrator) ||
			    await userManager.HasPermissionAsync(user, Permissions.ManageMessages)))
			return TypedResults.Problem(ProblemDetailsDefaults.CannotDeleteMessageSentByOtherUser);

		message.IsRemoved = true;
		message.ConcurrencyStamp = Guid.NewGuid().ToString("N");
		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new MessageDeletedEvent(message), CancellationToken.None);

		return TypedResults.NoContent();
	}
}
