using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Users;
using Aula.Server.Features.Users.Shared;
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

namespace Aula.Server.Features.Users.AddRole;

internal sealed class AddRoleEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("users/{userId}/roles/{roleId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRoles)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromRoute] Snowflake roleId,
		[FromServices] AppDbContext dbContext,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] IPublisher publisher,
		CancellationToken ct)
	{
		var user = await dbContext.Users
			.Where(u => u.Id == userId)
			.Include(u => u.RoleAssignments)
			.FirstOrDefaultAsync(ct);
		if (user is null)
			return TypedResults.NotFound();
		if (user.IsDeleted)
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsDeleted);

		var roleExists = await dbContext.Roles
			.Where(r => r.Id == roleId && !r.IsRemoved)
			.AnyAsync(ct);
		if (!roleExists)
			return TypedResults.NotFound();

		var alreadyAssigned = await dbContext.RoleAssignments
			.Where(r => r.RoleId == roleId && r.UserId == userId)
			.AnyAsync(ct);
		if (alreadyAssigned)
			return TypedResults.NoContent();

		user.RoleAssignments.Add(new RoleAssignment(await snowflakes.GenerateAsync(), roleId, userId));
		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new UserUpdatedEvent(user), CancellationToken.None);
		return TypedResults.NoContent();
	}
}
