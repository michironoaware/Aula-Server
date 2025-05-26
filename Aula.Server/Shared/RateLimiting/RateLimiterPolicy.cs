using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Aula.Server.Shared.RateLimiting;

internal sealed class RateLimiterPolicy
{
	private readonly Func<HttpContext, RateLimitPartition<DefaultKeyType>> _partitioner;

	internal RateLimiterPolicy(
		Func<HttpContext, RateLimitPartition<DefaultKeyType>> partitioner,
		Func<OnRejectedContext, CancellationToken, ValueTask>? onRejected)
	{
		_partitioner = partitioner;
		OnRejected = onRejected;
	}

	internal Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }

	internal RateLimitPartition<DefaultKeyType> GetPartition(HttpContext httpContext) => _partitioner(httpContext);
}
