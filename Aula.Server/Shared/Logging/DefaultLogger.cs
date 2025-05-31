namespace Aula.Server.Shared.Logging;

internal sealed class DefaultLogger : ILogger
{
	private readonly String _name;

	public DefaultLogger(String name)
	{
		_name = name;
	}

	public IDisposable? BeginScope<TState>(TState state)
		where TState : notnull =>
		null;

	public Boolean IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, String> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		var originalColor = Console.ForegroundColor;
		var logLevelColor = GetLogLevelColor(logLevel);
		var logLevelName = GetLogLevelString(logLevel);

		Console.ForegroundColor = logLevelColor;
		Console.Write($" [ {eventId.Id}: {logLevelName} ]");
		Console.ForegroundColor = ConsoleColor.DarkGray;
		Console.Write(" - ");
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.WriteLine(_name);
		Console.ForegroundColor = logLevelColor;
		Console.WriteLine($"{formatter(state, exception)}");
		Console.ForegroundColor = originalColor;
	}

	private static ConsoleColor GetLogLevelColor(LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Trace => ConsoleColor.DarkCyan,
			LogLevel.Debug => ConsoleColor.Cyan,
			LogLevel.Information => ConsoleColor.Green,
			LogLevel.Warning => ConsoleColor.Yellow,
			LogLevel.Error => ConsoleColor.Red,
			LogLevel.Critical => ConsoleColor.DarkRed,
			LogLevel.None or _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null),
		};
	}

	private static String GetLogLevelString(LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Trace => nameof(LogLevel.Trace),
			LogLevel.Debug => nameof(LogLevel.Debug),
			LogLevel.Information => nameof(LogLevel.Information),
			LogLevel.Warning => nameof(LogLevel.Warning),
			LogLevel.Error => nameof(LogLevel.Error),
			LogLevel.Critical => nameof(LogLevel.Critical),
			LogLevel.None or _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null),
		};
	}
}
