using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Bans;

internal abstract class Ban : DomainEntity
{
	internal const Int32 ReasonMinLength = 1;
	internal const Int32 ReasonMaxLength = 4096;

	[SetsRequiredMembers]
	private protected Ban(
		Snowflake id,
		BanType type,
		Snowflake? issuerId,
		String? reason)
	{
		Id = id;
		Type = type;
		IssuerType = issuerId is not null ? BanIssuerType.User : BanIssuerType.System;
		IssuerId = issuerId;
		Reason = reason;
		EmissionDate = DateTime.UtcNow;
		ConcurrencyStamp = GenerateConcurrencyStamp();
	}

	private protected Ban()
	{ }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public required Snowflake Id { get; init; }

	public required BanType Type { get; init; }

	public required BanIssuerType IssuerType { get; init; }

	public Snowflake? IssuerId { get; init; }

	[ForeignKey(nameof(IssuerId))]
	public virtual User? Issuer { get; init; }

	[Length(ReasonMinLength, ReasonMaxLength)]
	public String? Reason { get; init; }

	public required DateTime EmissionDate { get; init; }

	public required Boolean IsLifted { get; set; }

	[ConcurrencyCheck]
	public required String ConcurrencyStamp { get; set; }

	private static String GenerateConcurrencyStamp() => Guid.NewGuid().ToString("N");
}
