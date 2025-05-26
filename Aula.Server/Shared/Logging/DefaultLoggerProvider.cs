using System.Collections.Concurrent;

namespace Aula.Server.Shared.Logging;

internal sealed class DefaultLoggerProvider : ILoggerProvider
{
	private readonly ConcurrentDictionary<String, DefaultLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

	public ILogger CreateLogger(String categoryName)
	{
		return _loggers.GetOrAdd(categoryName, name => new DefaultLogger(name));
	}

	public void Dispose()
	{
		_loggers.Clear();
	}
}
