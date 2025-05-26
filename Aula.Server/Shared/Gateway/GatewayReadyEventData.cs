using Aula.Server.Shared.ApiContracts;

namespace Aula.Server.Shared.Gateway;

/// <summary>
///     Sent at the start of a gateway connection (not when reconnecting). Contains useful data for future use.
/// </summary>
internal sealed record GatewayReadyEventData
{
	/// <summary>
	///     The ID of the session.
	/// </summary>
	public required String SessionId { get; init; }

	/// <summary>
	///     The connected user.
	/// </summary>
	public required UserData User { get; init; }
}
