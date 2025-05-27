using Aula.Server.Features.Roles.GetRoles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Roles.Shared;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails CannotSetPermissionsNotHeld { get; } = new()
	{
		Title = "Missing permissions",
		Detail = "Cannot set permissions not held.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails HierarchyProblem { get; } = new()
	{
		Title = "Missing permissions",
		Detail = "The role to be modified belongs to a higher hierarchy.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidRoleCount { get; } = new()
	{
		Title = "Invalid role count",
		Detail =
			$"The role count must be between {GetRolesEndpoint.MinimumRoleCount} and {GetRolesEndpoint.MaximumRoleCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterRoleQueryParam { get; } = new()
	{
		Title = $"Invalid '{GetRolesEndpoint.AfterQueryParamName}' query parameter",
		Detail = "A role with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};

}
