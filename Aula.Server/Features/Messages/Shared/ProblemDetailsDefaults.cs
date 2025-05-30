using Aula.Server.Domain.Rooms;
using Aula.Server.Features.Messages.GetMessages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Messages.Shared;

internal static class ProblemDetailsDefaults
{
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
		Title = $"Invalid '{GetMessagesEndpoint.CountQueryParamName}' query parameter.",
		Detail =
			$"Must be between {GetMessagesEndpoint.MinimumMessageCount} and {GetMessagesEndpoint.MaximumMessageCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidBeforeMessage { get; } = new()
	{
		Title = $"Invalid '{GetMessagesEndpoint.BeforeQueryParamName}' query parameter.",
		Detail = "A message with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterMessage { get; } = new()
	{
		Title = $"Invalid '{GetMessagesEndpoint.AfterQueryParamName}' query parameter.",
		Detail = "A message with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails CannotDeleteMessageSentByOtherUser { get; } = new()
	{
		Title = "Missing permissions",
		Detail = "Message management permissions are required to delete a message sent by another user.",
		Status = StatusCodes.Status403Forbidden,
	};
}
