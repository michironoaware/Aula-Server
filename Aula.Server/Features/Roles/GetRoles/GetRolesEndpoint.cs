using Aula.Server.Features.Roles.Shared;
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

namespace Aula.Server.Features.Roles.GetRoles;

internal sealed class GetRolesEndpoint : IApiEndpoint
{
	internal const String CountQueryParamName = "count";
	internal const String AfterQueryParamName = "after";
	internal const Int32 MinimumRoleCount = 1;
	internal const Int32 MaximumRoleCount = 100;
	internal const Int32 DefaultRoleCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("roles", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<RoleData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = CountQueryParamName)] Int32? specifiedCount,
		[FromQuery(Name = AfterQueryParamName)] Snowflake? afterRoleId,
		[FromServices] AppDbContext dbContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var count = specifiedCount ?? DefaultRoleCount;
		if (count is > MaximumRoleCount or < MinimumRoleCount)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidRoleCount);

		var rolesQuery = dbContext.Roles
			.Where(r => !r.IsRemoved)
			.OrderBy(r => r.CreationDate)
			.AsQueryable();

		if (afterRoleId is not null)
		{
			var targetRole = await dbContext.Roles
				.Where(r => r.Id == afterRoleId && !r.IsRemoved)
				.Select(r => new { r.CreationDate })
				.FirstOrDefaultAsync(ct);
			if (targetRole is null)
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterRoleQueryParam);

			rolesQuery = rolesQuery.Where(r => r.CreationDate > targetRole.CreationDate);
		}

		var roles = await rolesQuery
			.Select(r => new RoleData
			{
				Id = r.Id,
				Name = r.Name,
				Permissions = r.Permissions,
				Position = r.Position,
				IsGlobal = r.IsGlobal,
				CreationDate = r.CreationDate,
			})
			.Take(count)
			.ToArrayAsync(ct);
		return TypedResults.Ok(roles.AsEnumerable());
	}
}
