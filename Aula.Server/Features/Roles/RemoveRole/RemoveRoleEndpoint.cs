using Aula.Server.Domain.AccessControl;
using Aula.Server.Features.Roles.Shared;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Roles.RemoveRole;

internal sealed class RemoveRoleEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("roles/{roleId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRoles)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		Snowflake roleId,
		AppDbContext dbContext,
		UserManager userManager,
		IPublisher publisher,
		HttpContext httpContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var currentUser = await userManager.GetUserAsync(httpContext.User);
		if (currentUser is null)
			return TypedResults.InternalServerError();

		var role = await dbContext.Roles
			.Where(r => r.Id == roleId && !r.IsRemoved && !r.IsGlobal)
			.FirstOrDefaultAsync(ct);
		if (role is null)
			return TypedResults.NoContent();

		var higherRole = currentUser.RoleAssignments
			.OrderByDescending(r => r.Role.Position)
			.Select(r => r.Role)
			.FirstOrDefault();
		if (await userManager.HasPermissionAsync(currentUser, Permissions.Administrator) ||
		    higherRole is null ||
		    (higherRole.Position <= role.Position &&
			    (higherRole.Position != role.Position || higherRole.Id >= role.Id)))
			return TypedResults.Problem(ProblemDetailsDefaults.HierarchyProblem);

		role.RoleAssignments.Clear();
		role.IsRemoved = true;
		role.ConcurrencyStamp = Guid.NewGuid().ToString("N");
		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new RoleRemovedEvent(role), CancellationToken.None);

		return TypedResults.NoContent();
	}
}
