using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Bans;
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

namespace Aula.Server.Features.Users.GetUserBans;

internal sealed class GetUserBansEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users/{userId}/bans", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<BanData>>, NotFound>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromServices] AppDbContext dbContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var bans = await dbContext.Bans
			.OfType<UserBan>()
			.Where(b => b.TargetUserId == userId)
			.Select(b => new BanData
			{
				Id = b.Id,
				Type = b.Type,
				IssuerType = b.IssuerType,
				IssuerId = b.IssuerId,
				Reason = b.Reason,
				TargetId = b.TargetUserId,
				IsLifted = b.IsLifted,
				EmissionDate = b.EmissionDate,
			})
			.ToArrayAsync(ct);

		return TypedResults.Ok(bans.AsEnumerable());
	}
}
