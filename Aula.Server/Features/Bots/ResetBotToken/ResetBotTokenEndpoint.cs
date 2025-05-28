using Aula.Server.Domain.Users;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using Aula.Server.Shared.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Bots.ResetBotToken;

internal sealed class ResetBotTokenEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("bots/{userId}/reset-token", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequireUserType(UserType.Standard)
			.RequirePermissions()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<ResetBotTokenResponseBody>, NotFound, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromServices] AppDbContext dbContext,
		[FromServices] ITokenProvider tokenProvider,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var user = await dbContext.Users
			.OfType<BotUser>()
			.Where(u => u.Id == userId && !u.IsDeleted)
			.FirstOrDefaultAsync(ct);
		if (user is null)
			return TypedResults.NotFound();

		user.SecurityStamp = Guid.NewGuid().ToString("N");

		_ = await dbContext.SaveChangesAsync(ct);

		return TypedResults.Ok(new ResetBotTokenResponseBody { Token = tokenProvider.CreateToken(user) });
	}
}
