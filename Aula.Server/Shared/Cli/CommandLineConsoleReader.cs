using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aula.Server.Shared.Cli;

/// <summary>
///     A background service that asynchronously reads from the console input stream and executes commands.
/// </summary>
internal sealed class CommandLineConsoleReader : BackgroundService
{
	private readonly CommandLine _commandLine;
	private readonly ILogger<CommandLineConsoleReader> _logger;

	public CommandLineConsoleReader(CommandLine commandLine, ILogger<CommandLineConsoleReader> logger)
	{
		_commandLine = commandLine;
		_logger = logger;
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Reviewed.")]
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// Console.In.ReadLineAsync is blocking, so we use the standard input stream directly and read it asynchronously.
		using var inputReader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
		while (await inputReader.ReadLineAsync(stoppingToken) is { } line)
		{
			try
			{
				_ = await _commandLine.ProcessCommandAsync(line.AsMemory(), stoppingToken);
			}
			catch (Exception ex)
			{
				// We catch the exception and do not rethrow it, as that would stop the process
				// and prevent the commandline service from processing further inputs.
				_logger.CommandFailed(ex, ex.GetType().ToString(), ex.Message, ex.StackTrace);
			}
		}
	}
}
