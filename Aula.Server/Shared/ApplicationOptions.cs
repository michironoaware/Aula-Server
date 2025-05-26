using System.ComponentModel.DataAnnotations;

namespace Aula.Server.Shared;

/// <summary>
///     General configurations for the application.
/// </summary>
internal sealed class ApplicationOptions
{
	internal const String SectionName = "Application";

	/// <summary>
	///     The publicly displayed name.
	/// </summary>
	[Required]
	public required String Name { get; set; }
}
