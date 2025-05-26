using Aula.Server.Domain.Users;
using Aula.Server.Features.Users.Shared;
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

namespace Aula.Server.Features.Users.GetUsers;

internal sealed class GetUsersEndpoint : IApiEndpoint
{
	internal const String TypeQueryParamName = "type";
	internal const String CountQueryParamName = "count";
	internal const String AfterQueryParamName = "after";
	internal const Int32 MinimumUserCount = 1;
	internal const Int32 MaximumUserCount = 100;
	internal const Int32 DefaultUserCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<UserData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = TypeQueryParamName)] UserType? specifiedType,
		[FromQuery(Name = CountQueryParamName)] Int32? specifiedCount,
		[FromQuery(Name = AfterQueryParamName)] Snowflake? specifiedAfter,
		[FromServices] AppDbContext dbContext)
	{
		var count = specifiedCount ?? DefaultUserCount;
		if (count is < MinimumUserCount or > MaximumUserCount)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidUserCount);

		var usersQuery = dbContext.Users
			.Where(u => !u.IsDeleted)
			.OrderBy(u => u.Id)
			.AsQueryable();

		if (specifiedType is not null)
			usersQuery = usersQuery.Where(u => u.Type == specifiedType);

		if (specifiedAfter is not null)
		{
			if (!await dbContext.Users.AnyAsync(m => m.Id == specifiedAfter))
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterUserQueryParam);

			usersQuery = usersQuery.Where(m => m.Id > specifiedAfter);
		}

		var users = await usersQuery
			.Select(u => new UserData
			{
				Id = u.Id,
				DisplayName = u.DisplayName,
				Description = u.Description,
				Type = u.Type,
				Presence = u.Presence,
				CurrentRoomId = u.CurrentRoomId,
				RoleIds = u.RoleAssignments.Select(r => r.RoleId),
			})
			.Take(count)
			.ToListAsync();
		return TypedResults.Ok(users.AsEnumerable());
	}
}
