using System.Text;
using Aula.Server.Shared.Cli;

namespace Aula.Server.Features.Commands.Help;

internal sealed partial class HelpCommand : Command
{
	private readonly CommandLine _commandLine;

	private readonly ILogger<HelpCommand> _logger;

	public HelpCommand(
		CommandLine commandLine,
		ILogger<HelpCommand> logger,
		IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		_commandLine = commandLine;
		_logger = logger;

		AddOptions(CommandOption);
	}

	internal override String Name => "help";

	internal override String Description => "Displays the list of available commands.";

	internal CommandOption CommandOption { get; } = new()
	{
		Name = "c", Description = "Show information about a specific command.", IsRequired = false, CanOverflow = true,
	};

	internal static String CreateHelpMessage(Command command)
	{
		const Int32 padding = 2;
		var message = new StringBuilder();
		var alignment = 16;

		var parameters = new CommandParameters();

		foreach (var parameter in command.Options.Select(static kvp => kvp.Value))
		{
			var name = $"{CommandOption.Prefix}${parameter.Name}";
			parameters.Options.Add(new ParameterInfo(name, parameter.Description));

			if (name.Length > alignment - padding)
				alignment = name.Length + padding;
		}

		foreach (var subCommand in command.SubCommands.Select(kvp => kvp.Value))
		{
			parameters.SubCommands.Add(new ParameterInfo(subCommand.Name, subCommand.Description));

			if (subCommand.Name.Length > alignment)
				alignment = subCommand.Name.Length;
		}

		if (command.Name.Length > alignment - padding)
			alignment = command.Name.Length + padding;

		_ = message
			.AppendLine()
			.Append(command.Name)
			.AppendLine(command.Description.PadLeft(command.Description.Length + alignment - command.Name.Length));

		if (parameters.Options.Count > 0)
			_ = message.AppendLine().AppendLine("OPTIONS: ");

		for (var i = 0; i < parameters.Options.Count; i++)
		{
			var param = parameters.Options[i];
			_ = message
				.Append(param.Name.PadLeft(param.Name.Length))
				.Append(param.Description.PadLeft(param.Description.Length + alignment - param.Name.Length));
			if (i < parameters.Options.Count - 1)
				_ = message.AppendLine();
		}

		if (parameters.SubCommands.Count > 0)
			_ = message.AppendLine().AppendLine("SUB-COMMANDS: ");

		for (var i = 0; i < parameters.SubCommands.Count; i++)
		{
			var param = parameters.SubCommands[i];
			_ = message
				.Append(param.Name.PadLeft(param.Name.Length))
				.Append(param.Description.PadLeft(param.Description.Length + alignment - param.Name.Length));
			if (i < parameters.SubCommands.Count - 1)
				_ = message.AppendLine();
		}

		return message.ToString();
	}

	internal static String CreateHelpMessage(params IReadOnlyList<Command> commands)
	{
		var message = new StringBuilder();
		var alignment = commands
			.Select(command => command.Name.Length)
			.Prepend(16)
			.Max();

		alignment++;

		_ = message.AppendLine();
		for (var i = 0; i < commands.Count; i++)
		{
			var command = commands[i];
			_ = message.Append(command.Name);
			_ = message.Append(
				command.Description.PadLeft(command.Description.Length + alignment - command.Name.Length));
			if (i < commands.Count - 1)
				_ = message.AppendLine();
		}

		return message.ToString();
	}

	internal override ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (!args.TryGetValue(CommandOption.Name, out var query) ||
		    String.IsNullOrWhiteSpace(query))
		{
			var commands = _commandLine.Commands
				.Select(static kvp => kvp.Value)
				.ToArray();
			LogHelpMessage(_logger, CreateHelpMessage(commands));
			return ValueTask.CompletedTask;
		}

		var querySegments = query.Split(' ');
		var commandName = querySegments[0];
		if (!_commandLine.Commands.TryGetValue(commandName, out var command))
		{
			LogUnknownCommandMessage(_logger, commandName);
			return ValueTask.CompletedTask;
		}

		foreach (var subCommandName in querySegments.Skip(1))
		{
			if (!command.SubCommands.TryGetValue(subCommandName, out var subCommand))
			{
				LogUnknownSubCommandMessage(_logger, subCommandName);
				return ValueTask.CompletedTask;
			}

			command = subCommand;
		}

		LogHelpMessage(_logger, CreateHelpMessage(command));
		return ValueTask.CompletedTask;
	}

	internal ValueTask Callback(String commandName, CancellationToken cancellationToken = default) =>
		Callback(new Dictionary<String, String> { { CommandOption.Name, commandName } }, cancellationToken);

	[LoggerMessage(LogLevel.Information, Message = "Here's a list of all available commands: {message}")]
	private static partial void LogHelpMessage(ILogger logger, String message);

	[LoggerMessage(LogLevel.Error, Message = "Unknown command: '{commandName}'")]
	private static partial void LogUnknownCommandMessage(ILogger logger, String commandName);

	[LoggerMessage(LogLevel.Error, Message = "Unknown sub-command: '{commandName}'")]
	private static partial void LogUnknownSubCommandMessage(ILogger logger, String commandName);

	private readonly struct CommandParameters()
	{
		internal List<ParameterInfo> Options { get; } = [ ];

		internal List<ParameterInfo> SubCommands { get; } = [ ];
	}

	private readonly record struct ParameterInfo(String Name, String Description);
}
