using System.Collections.Concurrent;

namespace Aula.Server.Shared.Cli;

/// <summary>
///     Provides a service for handling and executing command-line commands.
/// </summary>
internal sealed partial class CommandLine
{
	private readonly ConcurrentDictionary<String, Command> _commands = new();
	private readonly ILogger<CommandLine> _logger;

	public CommandLine(ILogger<CommandLine> logger)
	{
		_logger = logger;
	}

	/// <summary>
	///     A dictionary with the registered commands, where the <see cref="Command.Name" /> is the key.
	/// </summary>
	internal IReadOnlyDictionary<String, Command> Commands => _commands;

	/// <summary>
	///     Adds a new command to the command-line service.
	/// </summary>
	/// <param name="command">The command to register.</param>
	/// <exception cref="InvalidOperationException">Thrown if a command with the same name is already registered.</exception>
	internal void AddCommand(Command command)
	{
		if (!_commands.TryAdd(command.Name, command))
			throw new InvalidOperationException($"Command name already registered: '{command.Name}'.");
	}

	/// <summary>
	///     Processes a command asynchronously.
	/// </summary>
	/// <param name="input">The raw command input as a <see cref="ReadOnlyMemory{Char}" />.</param>
	/// <param name="cancellationToken">A cancellation token to observe while executing the command.</param>
	/// <returns>
	///     A task that resolves to <see langword="true" /> if the command was successfully executed; otherwise,
	///     <see langword="false" />.
	/// </returns>
	internal async ValueTask<Boolean> ProcessCommandAsync(
		ReadOnlyMemory<Char> input,
		CancellationToken cancellationToken = default) =>
		await ProcessCommandAsync(input, _commands, cancellationToken);

	[LoggerMessage(LogLevel.Error, Message = "Unknown command \"{name}\"")]
	private static partial void UnknownCommand(ILogger logger, String name);

	[LoggerMessage(LogLevel.Error, Message = "Invalid command option name \"{name}\"")]
	private static partial void InvalidCommandOption(ILogger logger, String name);

	[LoggerMessage(LogLevel.Error, Message = "The option \"{optionName}\" requires to provide a value.")]
	private static partial void MissingArgument(ILogger logger, String optionName);

	[LoggerMessage(LogLevel.Error, Message = "options {optionNames} are required but they were not provided.")]
	private static partial void LogMissingRequiredArguments(ILogger logger, String optionNames);

	private async ValueTask<Boolean> ProcessCommandAsync(
		ReadOnlyMemory<Char> input,
		IReadOnlyDictionary<String, Command> commands,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var inputSpan = input.Span;
		if (inputSpan.IsWhiteSpace())
			return false;

		var inputSegments = inputSpan.Split(' ');
		if (!inputSegments.MoveNext())
			return false;

		var commandName = inputSpan.Slice(inputSegments.Current.Start.Value, inputSegments.Current.End.Value)
			.ToString();
		if (!commands.TryGetValue(commandName, out var command))
		{
			UnknownCommand(_logger, commandName);
			return false;
		}

		var pendingOptions = command.Options
			.Select(kvp => kvp.Value)
			.Where(o => o.IsRequired)
			.ToList();
		var arguments = new Dictionary<String, String>();
		while (inputSegments.MoveNext())
		{
			cancellationToken.ThrowIfCancellationRequested();
			var segmentStart = inputSegments.Current.Start.Value;
			var segmentLength = inputSegments.Current.End.Value - segmentStart;

			if (segmentLength == 0)
				continue;

			var segment = input.Span.Slice(segmentStart, segmentLength);

			var startsWithParameterPrefix = segment.StartsWith(CommandOption.Prefix);
			if (!startsWithParameterPrefix &&
			    command.SubCommands.Count is not 0)
				// Should be a subcommand
			{
				return await ProcessCommandAsync(input.Slice(segmentStart, input.Length - segmentStart),
					command.SubCommands,
					cancellationToken);
			}

			CommandOption? option = null;
			if (startsWithParameterPrefix)
			{
				var optionName = segment[CommandOption.Prefix.Length..].ToString();
				if (!command.Options.TryGetValue(optionName, out option))
				{
					InvalidCommandOption(_logger, optionName);
					return false;
				}

				if (!option.RequiresArgument)
				{
					arguments.Add(option.Name, String.Empty);
					_ = pendingOptions.Remove(option);
					continue;
				}

				if (!inputSegments.MoveNext())
				{
					MissingArgument(_logger, option.Name);
					return false;
				}
			}

			if (option is null)
			{
				if (command.Options.Count is 1)
					// If a command has no subcommands, only one option, and no option is provided,
					// then that option is automatically selected.
				{
					option = command.Options
						.Select(kvp => kvp.Value)
						.First();
				}
				else
				{
					// The command multiple options, and we cannot guess which select.
					// returns the same response for unrecognized subcommands.
					UnknownCommand(_logger, commandName);
					return false;
				}
			}

			var argumentStart = inputSegments.Current.Start.Value;
			var argumentLength = inputSegments.Current.End.Value - argumentStart;
			if (option.CanOverflow)
			{
				while (inputSegments.MoveNext())
					argumentLength = inputSegments.Current.End.Value - argumentStart;
			}

			arguments.Add(option.Name, input.Slice(argumentStart, argumentLength).ToString());
			_ = pendingOptions.Remove(option);
		}

		if (pendingOptions.Count > 0)
		{
			LogMissingRequiredArguments(_logger, String.Join(", ", pendingOptions.Select(o => $"\"{o.Name}\"")));
			return false;
		}

		await command.Callback(arguments, cancellationToken);
		return true;
	}
}
