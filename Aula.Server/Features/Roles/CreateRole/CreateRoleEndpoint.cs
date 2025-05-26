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

namespace Aula.Server.Features.Roles.CreateRole;

internal sealed class CreateRoleEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("roles", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.ManageRoles)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Created<RoleData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		CreateRoleRequestBody body,
		IValidator<CreateRoleRequestBody> bodyValidator,
		AppDbContext dbContext,
		UserManager userManager,
		IPublisher publisher,
		ISnowflakeGenerator snowflakes,
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

		var role = new Role(await snowflakes.GenerateAsync(), body.Name ?? "New role", permissions, 1, false);
		_ = dbContext.Roles.Add(role);
		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new RoleCreatedEvent(role), CancellationToken.None);

		return TypedResults.Created($"{httpContext.Request.GetUrl()}/{role.Id}", role.ToRoleData());
	}
}
