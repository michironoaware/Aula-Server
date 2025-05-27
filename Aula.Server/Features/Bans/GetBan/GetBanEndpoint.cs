using Aula.Server.Domain.AccessControl;
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

namespace Aula.Server.Features.Bans.GetBan;

internal sealed class GetBanEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans/{banId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<BanData>, NotFound>> HandleAsync(
		[FromRoute] Snowflake banId,
		[FromServices] AppDbContext dbContext,
		HttpContext httpContext)
	{
		var ban = await dbContext.Bans
			.AsNoTracking()
			.Where(b => b.Id == banId)
			.FirstOrDefaultAsync();
		if (ban is null)
			return TypedResults.NotFound();

		return TypedResults.Ok(ban.ToBanData());
	}
}
