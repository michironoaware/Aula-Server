using Aula.Server.Features.Rooms.GetRooms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Rooms.Shared;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails InvalidRoomCount { get; } = new()
	{
		Title = "Invalid room count",
		Detail =
			$"The room count must be between {GetRoomsEndpoint.MinimumRoomCount} and {GetRoomsEndpoint.MaximumRoomCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterRoomQueryParam { get; } = new()
	{
		Title = $"Invalid '{GetRoomsEndpoint.AfterQueryParamName}' query parameter",
		Detail = "A room with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails DestinationRoomCannotBeSourceRoom { get; } = new()
	{
		Title = "Invalid destination room",
		Detail = "A destination room cannot be the same as the source room.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails DestinationRoomDoesNotExist { get; } = new()
	{
		Title = "Invalid destination room",
		Detail = "The specified destination room does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails OneDestinationRoomDoesNotExist { get; } = new()
	{
		Title = "Invalid destination room",
		Detail = "One of the specified destination rooms does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails BackgroundAudioFileDoesNotExist { get; } = new()
	{
		Title = "Invalid background audio file",
		Detail = "The background audio file does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};
}
