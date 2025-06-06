﻿using Aula.Server.Features.Users.GetUsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Users.Shared;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails SpecifiedCurrentRoomDoesNotExist { get; } = new()
	{
		Title = "Invalid room",
		Detail = "Cannot update the current room to a room that does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails RoomIsNotEntrance { get; } = new()
	{
		Title = "Invalid room",
		Detail = "The specified room is not an entrance.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails NoRoomConnection { get; } = new()
	{
		Title = "Room connection required",
		Detail = "The current room has no relationship with the specified destination room.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidUserCount { get; } = new()
	{
		Title = $"Invalid '{GetUsersEndpoint.CountQueryParamName}' query parameter",
		Detail = $"Must be between {GetUsersEndpoint.MinimumUserCount} and {GetUsersEndpoint.MaximumUserCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterUserQueryParam { get; } = new()
	{
		Title = $"Invalid '{GetUsersEndpoint.AfterQueryParamName}' query parameter",
		Detail = "A user with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails UserIsDeleted { get; } = new()
	{
		Title = "Invalid user",
		Detail = "The specified user was deleted.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails UserAlreadyBanned { get; } = new()
	{
		Title = "Invalid operation",
		Detail = "The specified user is already banned.",
		Status = StatusCodes.Status409Conflict,
	};

	internal static ProblemDetails TargetDoesNotExist { get; } = new()
	{
		Title = "Invalid target",
		Detail = "The specified user does not exist.",
		Status = StatusCodes.Status404NotFound,
	};

	internal static ProblemDetails TargetIsSelf { get; } = new()
	{
		Title = "Invalid target", Detail = "You cannot target yourself.", Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails TargetIsAdministrator { get; } = new()
	{
		Title = "Invalid operation",
		Detail = "The specified user has administrator permissions.",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails InsufficientPermissionsToModifyCurrentRoom { get; } = new()
	{
		Title = "Missing permissions",
		Detail = "Insufficient permissions to modify the target's current room.",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails InsufficientPermissionsToModifyRoles { get; } = new()
	{
		Title = "Missing permissions",
		Detail = "Insufficient permissions to modify the target's roles.",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails OneOrMoreRolesDoNotExist { get; } = new()
	{
		Title = "Invalid roles",
		Detail = "One or more of the specified roles does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails RoleAssignmentHierarchyProblem { get; } = new()
	{
		Title = "Missing permissions",
		Detail = "The role to assign belongs to a higher hierarchy.",
		Status = StatusCodes.Status403Forbidden,
	};
}
