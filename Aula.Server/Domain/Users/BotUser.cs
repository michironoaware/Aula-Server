using System.Diagnostics.CodeAnalysis;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Users;

internal sealed class BotUser : User
{
	[SetsRequiredMembers]
	internal BotUser(
		Snowflake id,
		String displayName,
		String description)
		: base(id, UserType.Bot, displayName, description)
	{ }

	// EntityFramework constructor
	private BotUser()
	{ }
}
