using Aula.Server.Domain.Users;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Features.Gateway.Events.UserPresenceUpdated;

internal sealed record UserPresenceUpdatedEventData
{
	public required Snowflake UserId { get; init; }

	public required Presence Presence { get; init; }
}
