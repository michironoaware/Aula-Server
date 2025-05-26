using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Domain.Files;
using Aula.Server.Domain.Messages;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Rooms;

internal abstract class Room : DomainEntity
{
	internal const Int32 NameMinLength = 3;
	internal const Int32 NameMaxLength = 32;
	internal const Int32 DescriptionMaxLength = 2048;

	[SetsRequiredMembers]
	private protected Room(
		Snowflake id,
		RoomType type,
		String name,
		String description,
		Boolean isEntrance,
		Snowflake? backgroundAudioId)
	{
		Id = id;
		Type = type;
		Name = name;
		Description = description;
		IsEntrance = isEntrance;
		BackgroundAudioId = backgroundAudioId;
		Destinations = [ ];
		Origins = [ ];
		Residents = [ ];
		Messages = [ ];
		CreationDate = DateTime.UtcNow;
		ConcurrencyStamp = GenerateConcurrencyStamp();
	}

	// EntityFramework constructor
	private protected Room()
	{ }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public required Snowflake Id { get; init; }

	public required RoomType Type { get; init; }

	[Length(NameMinLength, NameMaxLength)]
	public required String Name { get; set; }

	[MaxLength(DescriptionMaxLength)]
	public required String Description { get; set; }

	public required Boolean IsEntrance { get; set; }

	public Snowflake? BackgroundAudioId { get; set; }

	[ForeignKey(nameof(BackgroundAudioId))]
	public virtual File? BackgroundAudio { get; set; }

	public virtual List<RoomConnection> Origins { get; init; } = null!;

	public virtual List<RoomConnection> Destinations { get; init; } = null!;

	public virtual List<User> Residents { get; init; } = null!;

	public virtual List<Message> Messages { get; init; } = null!;

	public required DateTime CreationDate { get; init; }

	[ConcurrencyCheck]
	public required String ConcurrencyStamp { get; set; }

	public required Boolean IsRemoved { get; set; }

	private static String GenerateConcurrencyStamp() => Guid.NewGuid().ToString("N");
}
