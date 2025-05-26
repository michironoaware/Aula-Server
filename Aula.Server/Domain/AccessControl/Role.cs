using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.AccessControl;

internal class Role : DomainEntity
{
	internal const Int32 NameMinLength = 1;
	internal const Int32 NameMaxLength = 32;

	[SetsRequiredMembers]
	internal Role(
		Snowflake id,
		String name,
		Permissions permissions,
		Int32 position,
		Boolean isGlobal)
	{
		Id = id;
		Name = name;
		Permissions = permissions;
		Position = position;
		IsGlobal = isGlobal;
		RoleAssignments = [ ];
		CreationDate = DateTime.UtcNow;
		ConcurrencyStamp = GenerateConcurrencyStamp();
	}

	// EntityFramework constructor
	private Role()
	{ }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public required Snowflake Id { get; init; }

	[Length(NameMinLength, NameMaxLength)]
	public required String Name { get; set; }

	public required Permissions Permissions { get; set; }

	public required Int32 Position { get; set; }

	public required Boolean IsGlobal { get; set; }

	public virtual List<RoleAssignment> RoleAssignments { get; init; } = null!;

	public required DateTime CreationDate { get; init; }

	[ConcurrencyCheck]
	public required String ConcurrencyStamp { get; set; }

	public required Boolean IsRemoved { get; set; }

	private static String GenerateConcurrencyStamp() => Guid.NewGuid().ToString("N");
}
