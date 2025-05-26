using Aula.Server.Shared.Gateway;

namespace Aula.Server.Features.Gateway.Events.UpdatePresence;

/// <summary>
///     A request from a client to update the presence of its user.
/// </summary>
internal sealed record UpdatePresenceEventData
{
	/// <summary>
	///     The presence to use.
	/// </summary>
	public required PresenceOption Presence { get; init; }
}
