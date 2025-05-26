using Aula.Server.Domain;
using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Users;
using Aula.Server.Features.Users.Shared;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Users.ModifyUser;

internal sealed class ModifyUserEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPatch("users/{userId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<UserData>, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromBody] ModifyUserRequestBody body,
		[FromServices] IValidator<ModifyUserRequestBody> bodyValidator,
		[FromServices] AppDbContext dbContext,
		[FromServices] UserManager userManager,
		[FromServices] IPublisher publisher,
		[FromServices] ISnowflakeGenerator snowflakes,
		HttpContext httpContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var bodyValidation = await bodyValidator.ValidateAsync(body, ct);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var currentUser = await userManager.GetUserAsync(httpContext.User);
		if (currentUser is null)
			return TypedResults.InternalServerError();

		var user = await dbContext.Users
			.Where(u => u.Id == userId)
			.Include(u => u.RoleAssignments)
			.FirstOrDefaultAsync(ct);
		if (user is null)
			return TypedResults.NotFound();
		if (user.IsDeleted)
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsDeleted);

		var modified = false;
		var eventsToPublish = new List<DomainEvent>();
		if (body.DisplayName is not null &&
		    body.DisplayName != user.DisplayName &&
		    user.Id == currentUser.Id)
		{
			user.DisplayName = body.DisplayName;
			modified = true;
			if (eventsToPublish.All(e => e is not UserUpdatedEvent))
				eventsToPublish.Add(new UserUpdatedEvent(user));
		}

		if (body.Description is not null &&
		    body.Description != user.Description &&
		    user.Id == currentUser.Id)
		{
			user.Description = body.Description;
			modified = true;
			if (eventsToPublish.All(e => e is not UserUpdatedEvent))
				eventsToPublish.Add(new UserUpdatedEvent(user));
		}

		if (body.CurrentRoomId is not null &&
		    user.CurrentRoomId != body.CurrentRoomId)
		{
			if (user.Id != currentUser.Id &&
			    !await userManager.HasPermissionAsync(currentUser, Permissions.SetCurrentRoom) &&
			    !await userManager.HasPermissionAsync(currentUser, Permissions.Administrator))
				return TypedResults.Problem(ProblemDetailsDefaults.InsufficientPermissionsToModifyCurrentRoom);

			var room = await dbContext.Rooms
				.Where(r => r.Id == body.CurrentRoomId && !r.IsRemoved)
				.Select(r => new { r.IsEntrance })
				.FirstOrDefaultAsync(ct);
			if (room is null)
				return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);

			if (!await userManager.HasPermissionAsync(user, Permissions.Administrator))
			{
				if (user.CurrentRoomId is null && !room.IsEntrance)
					return TypedResults.Problem(ProblemDetailsDefaults.RoomIsNotEntrance);

				var hasConnection = await dbContext.RoomConnections.AnyAsync(r =>
					r.SourceRoomId == user.CurrentRoomId && r.DestinationRoomId == body.CurrentRoomId, ct);
				if (user.CurrentRoomId is not null && !hasConnection)
					return TypedResults.Problem(ProblemDetailsDefaults.NoRoomConnection);
			}

			var previousRoomId = user.CurrentRoomId;
			user.CurrentRoomId = body.CurrentRoomId;
			modified = true;
			eventsToPublish.Add(new UserCurrentRoomUpdatedEvent(user.Id, previousRoomId, body.CurrentRoomId));
		}

		if (body.RoleIds is not null)
		{
			if (!await userManager.HasPermissionAsync(currentUser, Permissions.ManageRoles) &&
			    !await userManager.HasPermissionAsync(currentUser, Permissions.Administrator))
				return TypedResults.Problem(ProblemDetailsDefaults.InsufficientPermissionsToModifyRoles);

			var userRoleIds = user.RoleAssignments.Select(ra => ra.RoleId).ToArray();
			var requestedRoleIds = body.RoleIds.Distinct().ToArray();
			var setRoleIds = requestedRoleIds.Where(id => userRoleIds.Contains(id)).ToArray();
			var unsetRoleIds = requestedRoleIds.Except(setRoleIds).ToArray();
			if (setRoleIds.Length != userRoleIds.Length ||
			    unsetRoleIds.Length is not 0)
			{
				var foundUnsetRolesCount = await dbContext.Roles
					.Where(r => unsetRoleIds.Contains(r.Id) && !r.IsRemoved && !r.IsGlobal)
					.CountAsync(ct);
				if (foundUnsetRolesCount < unsetRoleIds.Length)
					return TypedResults.Problem(ProblemDetailsDefaults.OneOrMoreRolesDoNotExist);

				var roleAssignments = unsetRoleIds.Select(id => new RoleAssignment(snowflakes.Generate(), id, userId));
				_ = user.RoleAssignments.RemoveAll(ra => !requestedRoleIds.Contains(ra.RoleId));
				user.RoleAssignments.AddRange(roleAssignments);

				if (eventsToPublish.All(e => e is not UserUpdatedEvent))
					eventsToPublish.Add(new UserUpdatedEvent(user));

				modified = true;
			}
		}

		if (modified)
		{
			user.ConcurrencyStamp = Guid.NewGuid().ToString("N");
			_ = await dbContext.SaveChangesAsync(ct);
			foreach (var ev in eventsToPublish)
				await publisher.Publish(ev, CancellationToken.None);
		}

		return TypedResults.Ok(user.ToUserData());
	}
}
