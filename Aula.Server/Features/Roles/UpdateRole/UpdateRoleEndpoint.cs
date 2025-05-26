using Aula.Server.Domain.AccessControl;
using Aula.Server.Features.Roles.Shared;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Roles.UpdateRole;

internal sealed class UpdateRoleEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPatch("roles/{roleId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRoles)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<RoleData>, ProblemHttpResult, NotFound, InternalServerError>> HandleAsync(
		Snowflake roleId,
		UpdateRoleRequestBody body,
		IValidator<UpdateRoleRequestBody> bodyValidator,
		AppDbContext dbContext,
		UserManager userManager,
		IPublisher publisher,
		HttpContext httpContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var validation = await bodyValidator.ValidateAsync(body, ct);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var permissions = body.Permissions ?? Permissions.None;

		var currentUser = await userManager.GetUserAsync(httpContext.User);
		if (currentUser is null)
			return TypedResults.InternalServerError();
		if (!await userManager.HasPermissionAsync(currentUser, Permissions.Administrator) &&
		    !await userManager.HasPermissionAsync(currentUser, permissions))
			return TypedResults.Problem(ProblemDetailsDefaults.CannotSetPermissionsNotHeld);

		var role = await dbContext.Roles
			.Where(r => r.Id == roleId && !r.IsRemoved)
			.FirstOrDefaultAsync(ct);
		if (role is null)
			return TypedResults.NotFound();

		var higherRole = currentUser.RoleAssignments
			.OrderByDescending(r => r.Role.Position)
			.Select(r => r.Role)
			.FirstOrDefault();
		if (await userManager.HasPermissionAsync(currentUser, Permissions.Administrator) ||
		    higherRole is null ||
		    (higherRole.Position <= role.Position &&
			    (higherRole.Position != role.Position || higherRole.Id >= role.Id)))
			return TypedResults.Problem(ProblemDetailsDefaults.HierarchyProblem);

		var modified = false;

		if (body.Name is not null)
		{
			role.Name = body.Name;
			modified = true;
		}

		if (body.Permissions is not null)
		{
			role.Permissions = (Permissions)body.Permissions!;
			modified = true;
		}

		if (modified)
		{
			role.ConcurrencyStamp = Guid.NewGuid().ToString("N");
			_ = await dbContext.SaveChangesAsync(ct);
			await publisher.Publish(new RoleUpdatedEvent(role), CancellationToken.None);
		}

		return TypedResults.Ok(role.ToRoleData());
	}
}
