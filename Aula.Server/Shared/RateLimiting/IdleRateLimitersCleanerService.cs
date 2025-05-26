namespace Aula.Server.Shared.RateLimiting;

internal sealed class IdleRateLimitersCleanerService : BackgroundService
{
	private static readonly TimeSpan s_interval = TimeSpan.FromMinutes(1);
	private readonly RateLimiterManager _rateLimiterManager;

	public IdleRateLimitersCleanerService(RateLimiterManager rateLimiterManager)
	{
		_rateLimiterManager = rateLimiterManager;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(s_interval, stoppingToken);
			_ = _rateLimiterManager.ClearIdlingRateLimiters();
		}
	}
}
