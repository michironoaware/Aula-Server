namespace Aula.Server.Features.Users.GetCurrentUserBanStatus;

internal sealed record GetCurrentUserBanStatusResponseBody
{
	public required Boolean Banned { get; init; }
}
