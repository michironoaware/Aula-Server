using Aula.Server.Domain.Bans;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Shared.ApiContracts;

internal sealed record BanData
{
	public required Snowflake Id { get; init; }

	public required BanType Type { get; init; }

	public required BanIssuerType IssuerType { get; init; }

	public Snowflake? IssuerId { get; init; }

	public String? Reason { get; init; }

	public Snowflake? TargetId { get; init; }

	public required Boolean IsLifted { get; init; }

	public required DateTime EmissionDate { get; init; }
}
