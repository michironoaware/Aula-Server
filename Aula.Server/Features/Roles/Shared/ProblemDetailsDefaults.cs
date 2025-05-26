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
}
