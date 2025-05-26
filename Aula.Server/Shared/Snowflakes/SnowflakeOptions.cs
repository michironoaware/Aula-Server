using System.ComponentModel.DataAnnotations;

namespace Aula.Server.Shared.Snowflakes;

internal sealed class SnowflakeOptions
{
	internal const String SectionName = "Application";

	/// <summary>
	///     The identifier for this worker, must be unique.
	/// </summary>
	[Range(0, Snowflake.MaxWorkerId)]
	public required UInt16 WorkerId { get; set; }
}
