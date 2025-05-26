using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Shared.ApiContracts;

internal sealed record FileData
{
	public required Snowflake Id { get; init; }

	public required String Name { get; init; }

	public required String ContentType { get; init; }

	public required Int64 Size { get; init; }
}
