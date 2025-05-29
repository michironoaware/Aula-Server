using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Features.Gateway.Events.MessageDeleted;

internal sealed record MessageDeletedEventData
{
	public Snowflake Id { get; init; }
}
