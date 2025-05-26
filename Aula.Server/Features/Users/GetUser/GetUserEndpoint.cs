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

namespace Aula.Server.Features.Users.GetUser;

internal sealed class GetUserEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users/{userId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<UserData>, NotFound>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromServices] UserManager userManager,
		[FromServices] AppDbContext dbContext,
		HttpContext httpContext)
	{
		var user = await dbContext.Users
			.Where(u => u.Id == userId)
			.Select(u =>
				new UserData
				{
					Id = userId,
					DisplayName = u.DisplayName,
					Description = u.Description,
					CurrentRoomId = u.CurrentRoomId,
					Type = u.Type,
					Presence = u.Presence,
					RoleIds = u.RoleAssignments.Select(ra => ra.Role.Id),
				})
			.FirstOrDefaultAsync();
		if (user is null)
			return TypedResults.NotFound();

		return TypedResults.Ok(user);
	}
}
