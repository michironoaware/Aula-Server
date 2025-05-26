using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace Aula.Server.Shared.Resilience;

internal static class DependencyInjection
{
	internal static IServiceCollection AddResilience(this IServiceCollection services)
	{
		_ = services.AddResiliencePipeline(ResiliencePipelines.RetryOnDbConcurrencyProblem, builder =>
		{
			_ = builder
				.AddRetry(new RetryStrategyOptions
				{
					UseJitter = true,
					MaxRetryAttempts = Int32.MaxValue - 1,
					ShouldHandle = new PredicateBuilder().Handle<DbUpdateConcurrencyException>(),
				});
		});

		return services;
	}
}
