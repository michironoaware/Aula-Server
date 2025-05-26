using System.ComponentModel.DataAnnotations.Schema;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.AccessControl;

internal class RoleAssignment : DomainEntity
{
	internal RoleAssignment(Snowflake id, Snowflake roleId, Snowflake userId)
	{
		Id = id;
		RoleId = roleId;
		UserId = userId;
	}

	public Snowflake Id { get; }

	public Snowflake RoleId { get; }

	public Snowflake UserId { get; }

	[ForeignKey(nameof(RoleId))]
	public virtual Role Role { get; init; } = null!;

	[ForeignKey(nameof(UserId))]
	public virtual User User { get; init; } = null!;
}
