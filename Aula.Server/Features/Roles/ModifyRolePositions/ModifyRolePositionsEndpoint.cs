using Aula.Server.Domain.AccessControl;
using Aula.Server.Features.Roles.Shared;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Roles.ModifyRolePositions;

internal sealed class ModifyRolePositionsEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPatch("roles", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRoles)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<RoleData>>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromBody] ModifyRolePositionsRequestBodyRole[] body,
		[FromServices] IValidator<ModifyRolePositionsRequestBodyRole> bodyRoleValidator,
		[FromServices] UserManager userManager,
		[FromServices] AppDbContext dbContext,
		[FromServices] IPublisher publisher,
		HttpContext httpContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		body = body
			.Distinct(RolePositionByIdEqualityComparer.Instance)
			.ToArray();
		var validations = new List<ValidationResult>();
		foreach (var roleBody in body)
			validations.Add(await bodyRoleValidator.ValidateAsync(roleBody, ct));
		if (validations.Any(v => !v.IsValid))
		{
			var errors = validations.SelectMany(v => v.Errors);
			var problemDetails = errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var currentUser = await userManager.GetUserAsync(httpContext.User);
		if (currentUser is null)
			return TypedResults.InternalServerError();
		var currentUserHigherRole = currentUser.RoleAssignments
			.OrderByDescending(r => r.Role.Position)
			.Select(r => r.Role)
			.FirstOrDefault();

		var maxRolePosition = await dbContext.Roles
			.Where(r => !r.IsRemoved)
			.CountAsync(ct) - 1;
		var rolesToModify = await dbContext.Roles
			.Where(r => body.Select(rb => rb.Id).Contains(r.Id) && !r.IsRemoved)
			.ToDictionaryAsync(r => r.Id, ct);

		var eventsToPublish = new List<RoleUpdatedEvent>();
		foreach (var roleBody in body)
		{
			var position = roleBody.Position > maxRolePosition ? maxRolePosition : roleBody.Position;
			var role = rolesToModify[roleBody.Id];

			if (await userManager.HasPermissionAsync(currentUser, Permissions.Administrator) ||
			    currentUserHigherRole is null ||
			    (currentUserHigherRole.Position <= role.Position &&
				    (currentUserHigherRole.Position != role.Position || currentUserHigherRole.Id >= role.Id)))
				return TypedResults.Problem(ProblemDetailsDefaults.HierarchyProblem);

			if (role.Position != position)
			{
				role.Position = position;
				role.ConcurrencyStamp = Guid.NewGuid().ToString("N");
				eventsToPublish.Add(new RoleUpdatedEvent(role));
			}
		}

		_ = await dbContext.SaveChangesAsync(ct);
		foreach (var ev in eventsToPublish)
			await publisher.Publish(ev, CancellationToken.None);

		var allRoles = await dbContext.Roles
			.AsNoTracking()
			.Where(r => !r.IsRemoved)
			.ToArrayAsync(ct);
		return TypedResults.Ok(allRoles.Select(r => r.ToRoleData()));
	}
}
