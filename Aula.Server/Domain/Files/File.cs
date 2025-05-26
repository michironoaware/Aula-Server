using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Domain.Rooms;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Files;

internal class File : DomainEntity
{
	internal const Int32 NameMinLength = 1;
	internal const Int32 NameMaxLength = 128;
	internal const Int32 ContentTypeMaxLength = 64;

	[SetsRequiredMembers]
	internal File(
		Snowflake id,
		String name,
		String contentType,
		Byte[] content,
		Snowflake submitterId)
	{
		Id = id;
		Name = name;
		ContentType = contentType;
		Content = content;
		Size = content.Length;
		SubmitterId = submitterId;
		ChatsUsingAsBackgroundAudio = [ ];
		UploadDate = DateTime.UtcNow;
		ConcurrencyStamp = GenerateConcurrencyStamp();
	}

	// EntityFramework constructor
	private File()
	{ }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public required Snowflake Id { get; init; }

	[Length(NameMinLength, NameMaxLength)]
	public required String Name { get; set; }

	[MaxLength(ContentTypeMaxLength)]
	public required String ContentType { get; init; }

	public required Byte[] Content { get; init; }

	public required Int64 Size { get; init; }

	public required Snowflake SubmitterId { get; init; }

	[ForeignKey(nameof(SubmitterId))]
	public virtual User Submitter { get; init; } = null!;

	public virtual List<Room> ChatsUsingAsBackgroundAudio { get; init; } = null!;

	public required DateTime UploadDate { get; init; }

	public required String ConcurrencyStamp { get; set; }

	public required Boolean IsRemoved { get; set; }

	private static String GenerateConcurrencyStamp() => Guid.NewGuid().ToString("N");
}
