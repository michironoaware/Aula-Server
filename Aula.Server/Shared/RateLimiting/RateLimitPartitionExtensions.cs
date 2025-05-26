using System.Threading.RateLimiting;

namespace Aula.Server.Shared.RateLimiting;

internal static class RateLimitPartitionExtensions
{
	internal static RateLimitPartition<TKey> GetExtendedFixedWindowRateLimiter<TKey>(
		TKey partitionKey,
		Func<TKey, FixedWindowRateLimiterOptions> factory)
	{
		return new RateLimitPartition<TKey>(partitionKey, key =>
		{
			var options = factory(key);

			// The ExtendedReplenishingRateLimiter will handle the replenishment
			if (options.AutoReplenishment)
			{
				options = new FixedWindowRateLimiterOptions
				{
					PermitLimit = options.PermitLimit,
					QueueProcessingOrder = options.QueueProcessingOrder,
					QueueLimit = options.QueueLimit,
					Window = options.Window,
					AutoReplenishment = false,
				};
			}

			return new ExtendedReplenishingRateLimiter(new FixedWindowRateLimiter(options));
		});
	}
}
