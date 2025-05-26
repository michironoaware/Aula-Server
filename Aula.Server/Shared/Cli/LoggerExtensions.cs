namespace Aula.Server.Shared.Cli;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Error, Message = "Command failed: {message}")]
	internal static partial void CommandFailed(this ILogger logger, String message);

	[LoggerMessage(LogLevel.Error, Message = "Command failed with an exception.")]
	internal static partial void CommandFailed(this ILogger logger, Exception exception);
}
