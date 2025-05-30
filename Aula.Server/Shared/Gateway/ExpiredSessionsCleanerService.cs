namespace Aula.Server.Shared.Gateway;

internal sealed partial class ExpiredSessionsCleanerService : BackgroundService
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
	private readonly ILogger<ExpiredSessionsCleanerService> _logger;

	public ExpiredSessionsCleanerService(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<ExpiredSessionsCleanerService> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		using var scope = _serviceScopeFactory.CreateScope();
		var gatewayManager = scope.ServiceProvider.GetRequiredService<GatewayManager>();
		_cleanupInterval = gatewayManager.ExpirePeriod < _cleanupInterval
			? gatewayManager.ExpirePeriod
			: _cleanupInterval;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var gatewayManager = scope.ServiceProvider.GetRequiredService<GatewayManager>();
			await Task.Delay(_cleanupInterval, stoppingToken);
			LogCleanup(_logger, gatewayManager.ClearExpiredSessions());
		}
	}

	[LoggerMessage(LogLevel.Debug, "{sessionsCleared} expired gateway sessions cleared.")]
	private static partial void LogCleanup(ILogger l, Int32 sessionsCleared);
}
