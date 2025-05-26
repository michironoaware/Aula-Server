using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Gateway.ConnectToGateway;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails NotAGatewayRequest { get; } = new()
	{
		Title = "Request problem.",
		Detail = "Request is not a WebSocket establishment request.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidSession { get; } = new()
	{
		Title = "Invalid session id.",
		Detail = "The session does not exist, is active, or has expired, or the current user does not own it",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails OnlyAdminCanProvidePresence { get; } = new()
	{
		Title = "Missing permissions.",
		Detail = "Only users with administrator permissions can provide and set a presence.",
		Status = StatusCodes.Status400BadRequest,
	};
}
