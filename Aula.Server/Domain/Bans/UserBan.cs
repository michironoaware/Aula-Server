using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Bans;

internal class UserBan : Ban
{
	[SetsRequiredMembers]
	internal UserBan(
		Snowflake id,
		Snowflake? issuerId,
		String? reason,
		Snowflake targetUserId)
		: base(id, BanType.User, issuerId, reason)
	{
		TargetUserId = targetUserId;
	}

	private UserBan()
	{ }

	public required Snowflake TargetUserId { get; init; }

	[ForeignKey(nameof(TargetUserId))]
	public virtual User TargetUser { get; init; } = null!;
}
