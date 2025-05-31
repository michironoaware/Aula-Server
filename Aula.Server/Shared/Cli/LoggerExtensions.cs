namespace Aula.Server.Shared.Cli;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Error, Message = "Command failed: {message}")]
	internal static partial void CommandFailed(this ILogger logger, String message);

	[LoggerMessage(LogLevel.Error, "Command failed with an exception." +
		" {exceptionType}:  \"{exceptionMessage}\"" +
		"\nStack trace: {exceptionStack}")]
	internal static partial void CommandFailed(
		this ILogger logger,
		Exception exception,
		String exceptionType,
		String exceptionMessage,
		String? exceptionStack);
}
