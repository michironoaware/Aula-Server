namespace Aula.Server.Shared.Identity;

internal sealed partial class EmailConfirmationCleanupService : BackgroundService
{
	private readonly ILogger<EmailConfirmationCleanupService> _logger;

	public EmailConfirmationCleanupService(ILogger<EmailConfirmationCleanupService> logger)
	{
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
			LogCleanup(_logger, UserManager.ClearExpiredEmailConfirmations());
		}
	}

	[LoggerMessage(LogLevel.Debug, "{confirmationsCleared} expired email confirmations cleared.")]
	private static partial void LogCleanup(ILogger l, Int32 confirmationsCleared);
}
