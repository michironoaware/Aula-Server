using Aula.Server.Domain.Rooms;
using Aula.Server.Features.Messages.GetMessages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Messages.Shared;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails RoomDoesNotExist { get; } = new()
	{
		Title = "Invalid room",
		Detail = "The specified room does not exist",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidRoomType { get; } = new()
	{
		Title = "Invalid room",
		Detail = $"Room must be of type ${(Int32)RoomType.Standard}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails UserIsNotInTheRoom { get; } = new()
	{
		Title = "Invalid room",
		Detail = "The current user is not in the room",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails UserIsNotInTheRoomAndNoAdministrator { get; } = new()
	{
		Title = "Invalid room",
		Detail = "The current user is not in the room and has no administrator permissions.",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails InvalidMessageType { get; } = new()
	{
		Title = "Invalid message type",
		Detail = "Cannot send messages of the specified type.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidMessageCount { get; } = new()
	{
		Title = "Invalid message count.",
		Detail =
			$"The message count must be between {GetMessagesEndpoint.MinimumMessageCount} and {GetMessagesEndpoint.MaximumMessageCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidBeforeMessage { get; } = new()
	{
		Title = $"Invalid '{GetMessagesEndpoint.BeforeQueryParameter}' query parameter.",
		Detail = "A message with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterMessage { get; } = new()
	{
		Title = $"Invalid '{GetMessagesEndpoint.AfterQueryParameter}' query parameter.",
		Detail = "A message with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};
}
