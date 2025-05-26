using Aula.Server.Features.Files.GetFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Files.Shared;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails InvalidFileCount { get; } = new()
	{
		Title = "Invalid file count.",
		Detail =
			$"The file count must be between {GetFilesEndpoint.MinimumMessageCount} and {GetFilesEndpoint.MaximumMessageCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterFile { get; } = new()
	{
		Title = $"Invalid '{GetFilesEndpoint.AfterQueryParameter}' query parameter.",
		Detail = "A file with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};
}
