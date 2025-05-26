using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Messages;

internal sealed class DefaultMessage : Message
{
	internal const Int32 TextMinLength = 1;
	internal const Int32 TextMaxLength = 2048;

	[SetsRequiredMembers]
	internal DefaultMessage(
		Snowflake id,
		MessageFlags flags,
		Snowflake? authorId,
		Snowflake roomId,
		String text)
		: base(id, MessageType.Default, flags, authorId, roomId)
	{
		Text = text;
	}

	// EntityFramework constructor
	private DefaultMessage()
	{ }

	[Length(TextMinLength, TextMaxLength)]
	public required String Text { get; set; }
}
