using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Identity.Shared;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails UserDoesNotExist { get; } = new()
	{
		Title = "Invalid user",
		Detail = "The specified user does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails IncorrectPassword { get; } = new()
	{
		Title = "Login problem",
		Detail = "The password provided is incorrect.",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails UserIsLockedOut { get; } = new()
	{
		Title = "Login problem",
		Detail = "The account is temporarily locked out due to multiple unsuccessful login attempts.",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails EmailNotConfirmed { get; } = new()
	{
		Title = "Login problem",
		Detail = "The email must be confirmed to login.",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails InvalidResetPasswordToken { get; } = new()
	{
		Title = "Invalid reset password code",
		Detail = "The reset password code is invalid.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidBase64UrlEmail { get; } = new()
	{
		Title = "Invalid email",
		Detail = "The email must be encoded and sent in base64url.",
		Status = StatusCodes.Status400BadRequest,
	};
}
