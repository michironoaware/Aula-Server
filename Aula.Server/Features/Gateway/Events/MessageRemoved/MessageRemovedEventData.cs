using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Features.Gateway.Events.MessageRemoved;

internal sealed record MessageRemovedEventData
{
	public Snowflake Id { get; init; }
}
