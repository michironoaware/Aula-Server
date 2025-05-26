using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Domain.Rooms;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Messages;

internal abstract class Message : DomainEntity
{
	[SetsRequiredMembers]
	private protected Message(
		Snowflake id,
		MessageType type,
		MessageFlags flags,
		Snowflake? authorId,
		Snowflake roomId)
	{
		Id = id;
		Type = type;
		Flags = flags;
		AuthorId = authorId;
		RoomId = roomId;
		CreationDate = DateTime.UtcNow;
		ConcurrencyStamp = GenerateConcurrencyStamp();
	}

	// EntityFramework constructor
	private protected Message()
	{ }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public required Snowflake Id { get; init; }

	public required MessageType Type { get; init; }

	public required MessageFlags Flags { get; init; }

	public required MessageAuthorType AuthorType { get; init; }

	public Snowflake? AuthorId { get; init; }

	[ForeignKey(nameof(AuthorId))]
	public virtual User? Author { get; init; }

	public required Snowflake RoomId { get; init; }

	[ForeignKey(nameof(RoomId))]
	public virtual Room Room { get; init; } = null!;

	public required DateTime CreationDate { get; init; }

	public required Boolean IsRemoved { get; set; }

	[ConcurrencyCheck]
	public required String ConcurrencyStamp { get; set; }

	private static String GenerateConcurrencyStamp() => Guid.NewGuid().ToString("N");
}
