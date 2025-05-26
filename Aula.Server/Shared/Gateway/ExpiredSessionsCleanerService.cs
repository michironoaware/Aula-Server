namespace Aula.Server.Shared.Gateway;

internal sealed partial class ExpiredSessionsCleanerService : BackgroundService
{
	private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
	private readonly GatewayManager _gatewayManager;
	private readonly ILogger<ExpiredSessionsCleanerService> _logger;

	public ExpiredSessionsCleanerService(GatewayManager gatewayManager, ILogger<ExpiredSessionsCleanerService> logger)
	{
		_gatewayManager = gatewayManager;
		_logger = logger;
		_cleanupInterval = gatewayManager.ExpirePeriod < _cleanupInterval
			? gatewayManager.ExpirePeriod
			: _cleanupInterval;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(_cleanupInterval, stoppingToken);
			LogCleanup(_logger, _gatewayManager.ClearExpiredSessions());
		}
	}

	[LoggerMessage(LogLevel.Debug, "{sessionsCleared} expired gateway sessions cleared.")]
	private static partial void LogCleanup(ILogger l, Int32 sessionsCleared);
}
