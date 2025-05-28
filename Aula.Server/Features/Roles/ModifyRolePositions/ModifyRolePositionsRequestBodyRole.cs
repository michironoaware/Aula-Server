using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Features.Roles.ModifyRolePositions;

internal sealed record ModifyRolePositionsRequestBodyRole
{
	public required Snowflake Id { get; init; }

	public required Int32 Position { get; init; }
}
