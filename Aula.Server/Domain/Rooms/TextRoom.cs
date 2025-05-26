using System.Diagnostics.CodeAnalysis;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Rooms;

internal class TextRoom : Room
{
	[SetsRequiredMembers]
	internal TextRoom(
		Snowflake id,
		String name,
		String description,
		Boolean isEntrance,
		Snowflake? backgroundAudioId)
		: base(id, RoomType.Standard, name, description, isEntrance, backgroundAudioId)
	{ }

	// EntityFramework constructor
	private TextRoom()
	{ }
}
