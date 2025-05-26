using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Features.Roles.UpdateRolePositions;

internal sealed record UpdateRolePositionsRequestBodyRole
{
	public required Snowflake Id { get; init; }

	public required Int32 Position { get; init; }
}
