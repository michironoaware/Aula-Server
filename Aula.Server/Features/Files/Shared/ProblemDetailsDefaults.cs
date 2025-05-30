using Aula.Server.Features.Files.GetFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Files.Shared;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails InvalidFileCount { get; } = new()
	{
		Title = $"Invalid '{GetFilesEndpoint.CountQueryParamName}' query parameter.",
		Detail = $"Must be between {GetFilesEndpoint.MinimumMessageCount} and {GetFilesEndpoint.MaximumMessageCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterFile { get; } = new()
	{
		Title = $"Invalid '{GetFilesEndpoint.AfterQueryParamName}' query parameter.",
		Detail = "A file with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};
}
