using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Files;
using Aula.Server.Domain.Messages;
using Aula.Server.Domain.Rooms;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Users;

internal abstract class User : DomainEntity
{
	internal const String DisplayNameValidChars = $"{UppercaseCharacters}{LowercaseCharacters}{Digits}._ ";
	internal const Int32 DisplayNameMinLength = 3;
	internal const Int32 DisplayNameMaxLength = 32;
	internal const Int32 DescriptionMaxLength = 2048;
	private const String UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private const String LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
	private const String Digits = "0123456789";

	[SetsRequiredMembers]
	private protected User(
		Snowflake id,
		UserType type,
		String displayName,
		String description)
	{
		Id = id;
		Type = type;
		DisplayName = displayName;
		Description = description;
		CreationDate = DateTime.UtcNow;
		RoleAssignments = [ ];
		MessagesSent = [ ];
		BansIssued = [ ];
		BansReceived = [ ];
		FilesSubmitted = [ ];
		ConcurrencyStamp = GenerateConcurrencyStamp();
	}

	// EntityFramework constructor
	private protected User()
	{ }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public required Snowflake Id { get; init; }

	public required UserType Type { get; init; }

	[Length(DisplayNameMinLength, DisplayNameMaxLength)]
	public required String DisplayName { get; set; }

	[MaxLength(DescriptionMaxLength)]
	public required String Description { get; set; }

	public required Presence Presence { get; set; }

	public Snowflake? CurrentRoomId { get; set; }

	[ForeignKey(nameof(CurrentRoomId))]
	public virtual Room? CurrentRoom { get; set; }

	public virtual List<RoleAssignment> RoleAssignments { get; init; } = null!;

	public virtual List<Message> MessagesSent { get; init; } = null!;

	public virtual List<Ban> BansIssued { get; init; } = null!;

	public virtual List<UserBan> BansReceived { get; init; } = null!;

	public virtual List<File> FilesSubmitted { get; init; } = null!;

	public required DateTime CreationDate { get; init; }

	public String? SecurityStamp { get; set; }

	public required Boolean IsDeleted { get; set; }

	[ConcurrencyCheck]
	public required String ConcurrencyStamp { get; set; }

	[Obsolete("Need to check if selecting this through Queryable.Select has unexpected behavior.")]
	public Permissions Permissions => RoleAssignments.Select(ra => ra.Role.Permissions).Aggregate((a, b) => a | b);

	private static String GenerateConcurrencyStamp() => Guid.NewGuid().ToString("N");
}
