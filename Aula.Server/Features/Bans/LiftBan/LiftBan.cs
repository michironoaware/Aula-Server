using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Bans;
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

namespace Aula.Server.Features.Bans.LiftBan;

internal sealed class LiftBan : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("bans/{banId}/lift", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, NotFound, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake banId,
		[FromServices] AppDbContext dbContext,
		[FromServices] IPublisher publisher,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var ban = await dbContext.Bans
			.Where(b => b.Id == banId)
			.FirstOrDefaultAsync(ct);
		switch (ban)
		{
			case null: return TypedResults.NotFound();
			case { IsLifted: true }: return TypedResults.NoContent();
		}

		ban.IsLifted = true;
		ban.ConcurrencyStamp = Guid.NewGuid().ToString("N");
		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new BanLiftedEvent(ban), CancellationToken.None);

		return TypedResults.NoContent();
	}
}
