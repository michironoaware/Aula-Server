using Aula.Server.Features.Bans.GetBans;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Bans.Shared;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails InvalidBanCount { get; } = new()
	{
		Title = "Invalid ban count.",
		Detail =
			$"The ban count must be between {GetBansEndpoint.MinimumBanCount} and {GetBansEndpoint.MaximumBanCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterBan { get; } = new()
	{
		Title = $"Invalid '{GetBansEndpoint.AfterQueryParamName}' query parameter.",
		Detail = "A ban with the specified user ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};
}
