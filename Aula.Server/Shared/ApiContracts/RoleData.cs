using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Shared.ApiContracts;

internal sealed record RoleData
{
	public required Snowflake Id { get; init; }

	public required String Name { get; init; }

	public required Permissions Permissions { get; init; }

	public required Int32 Position { get; init; }

	public required Boolean IsGlobal { get; init; }

	public required DateTime CreationDate { get; init; }
}
