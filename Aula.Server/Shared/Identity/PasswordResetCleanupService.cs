namespace Aula.Server.Shared.Identity;

internal sealed partial class PasswordResetCleanupService : BackgroundService
{
	private readonly ILogger<PasswordResetCleanupService> _logger;

	public PasswordResetCleanupService(ILogger<PasswordResetCleanupService> logger)
	{
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
			LogCleanup(_logger, UserManager.ClearExpiredPasswordResets());
		}
	}

	[LoggerMessage(LogLevel.Debug, "{resetsCleared} password resets cleared.")]
	private static partial void LogCleanup(ILogger l, Int32 resetsCleared);
}
