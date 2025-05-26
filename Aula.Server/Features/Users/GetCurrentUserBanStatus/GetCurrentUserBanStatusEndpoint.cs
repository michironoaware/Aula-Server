using Aula.Server.Domain.Bans;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Users.GetCurrentUserBanStatus;

internal sealed class GetCurrentUserBanStatusEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users/@me/ban-status", HandleAsync)
			.RequireAuthenticatedUser()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<GetCurrentUserBanStatusResponseBody>, InternalServerError>> HandleAsync(
		[FromServices] UserManager userManager,
		[FromServices] AppDbContext dbContext,
		HttpContext httpContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var userId = userManager.GetUserId(httpContext.User);
		if (userId is null)
			return TypedResults.InternalServerError();

		return TypedResults.Ok(new GetCurrentUserBanStatusResponseBody
		{
			Banned = await dbContext.Bans
				.OfType<UserBan>()
				.Where(x => x.TargetUserId == userId)
				.AnyAsync(ct),
		});
	}
}
