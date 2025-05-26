using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Domain;

internal abstract class DomainEntity : IValidatableObject
{
	[NotMapped]
	[SuppressMessage("Performance", "CA1822:Mark members as static",
		Justification = "Defined as instance member to support polymorphism and future instance-level logic.")]
	[Obsolete]
	internal List<DomainEvent> Events => [ ];

	[NotMapped]
	[Obsolete]
	internal Boolean DeleteFromDbOnSave { get; set; }

	[Obsolete]
	public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		yield break;
	}
}
