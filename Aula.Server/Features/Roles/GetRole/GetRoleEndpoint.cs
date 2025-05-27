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

namespace Aula.Server.Features.Roles.GetRole;

internal sealed class GetRoleEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("role/{roleId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<RoleData>, NotFound>> HandleAsync(
		[FromRoute] Snowflake roleId,
		[FromServices] AppDbContext dbContext,
		HttpContext httpContext)
	{
		var role = await dbContext.Roles
			.Where(r => r.Id == roleId)
			.Select(r =>
				new RoleData
				{
					Id = r.Id,
					Name = r.Name,
					Permissions = r.Permissions,
					Position = r.Position,
					IsGlobal = r.IsGlobal,
				})
			.FirstOrDefaultAsync();
		if (role is null)
			return TypedResults.NotFound();

		return TypedResults.Ok(role);
	}
}
