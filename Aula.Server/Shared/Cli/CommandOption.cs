namespace Aula.Server.Shared.Cli;

/// <summary>
///     Represents a <see cref="Command" /> option.
/// </summary>
internal sealed class CommandOption
{
	/// <summary>
	///     The prefix used to identify an option in the command line.
	/// </summary>
	internal const String Prefix = "-";

	/// <summary>
	///     The name of the command option.
	/// </summary>
	internal required String Name { get; init; }

	/// <summary>
	///     A brief description of the command option, explaining its purpose.
	/// </summary>
	internal required String Description { get; init; }

	/// <summary>
	///     Indicates whether this option is mandatory.
	/// </summary>
	internal Boolean IsRequired { get; init; } = true;

	/// <summary>
	///     Specifies whether the option requires an additional argument.
	/// </summary>
	internal Boolean RequiresArgument { get; init; } = true;

	/// <summary>
	///     Determines whether the option can accept multiple values beyond the initial argument.
	/// </summary>
	internal Boolean CanOverflow { get; init; }
}
