using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Users;
using Aula.Server.Features.Users.Shared;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Users.RemoveRole;

internal sealed class RemoveRoleEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("users/{userId}/role/{roleId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRoles)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromRoute] Snowflake roleId,
		[FromServices] AppDbContext dbContext,
		[FromServices] UserManager userManager,
		[FromServices] IPublisher publisher,
		HttpContext httpContext,
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

		var role = await dbContext.Roles
			.Where(r => r.Id == roleId && !r.IsRemoved)
			.Select(r => new { r.Id, r.Position })
			.FirstOrDefaultAsync(ct);
		if (role is null)
			return TypedResults.NotFound();

		var currentUser = await userManager.GetUserAsync(httpContext.User);
		if (currentUser is null)
			return TypedResults.InternalServerError();

		var higherRole = currentUser.RoleAssignments
			.OrderByDescending(r => r.Role.Position)
			.Select(r => r.Role)
			.FirstOrDefault();
		if (await userManager.HasPermissionAsync(currentUser, Permissions.Administrator) ||
		    higherRole is null ||
		    (higherRole.Position <= role.Position &&
			    (higherRole.Position != role.Position || higherRole.Id >= role.Id)))
			return TypedResults.Problem(ProblemDetailsDefaults.RoleAssignmentHierarchyProblem);

		var roleAssignmentIndex = user.RoleAssignments.FindIndex(r => r.RoleId == roleId && r.UserId == userId);
		if (roleAssignmentIndex is -1)
			return TypedResults.NoContent();
		user.RoleAssignments.RemoveAt(roleAssignmentIndex);

		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new UserUpdatedEvent(user), CancellationToken.None);
		return TypedResults.NoContent();
	}
}
