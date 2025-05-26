using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Aula.Server.Shared.RateLimiting;

internal sealed class RateLimiterOptions
{
	private readonly Dictionary<String, RateLimiterPolicy> _policyMap = new(StringComparer.Ordinal);

	internal IReadOnlyDictionary<String, RateLimiterPolicy> PolicyMap => _policyMap;

	internal RateLimiterPolicy? GlobalPolicy { get; private set; }

	/// <summary>
	///     Gets or sets a <see cref="Func{OnRejectedContext, CancellationToken, ValueTask}" /> that handles requests rejected
	///     by this middleware.
	/// </summary>
	internal Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; set; }

	/// <summary>
	///     Gets or sets the default status code to set on the response when a request is rejected.
	///     Defaults to <see cref="StatusCodes.Status503ServiceUnavailable" />.
	/// </summary>
	/// <remarks>
	///     This status code will be set before <see cref="OnRejected" /> is called, so any status code set by
	///     <see cref="OnRejected" /> will "win" over this default.
	/// </remarks>
	internal Int32 RejectionStatusCode { get; set; } = StatusCodes.Status503ServiceUnavailable;

	/// <summary>
	///     Adds a new rate limiting policy with the given <paramref name="policyName" />
	/// </summary>
	/// <param name="policyName">The name to be associated with the given <see cref="RateLimiter" />.</param>
	/// <param name="partitioner">
	///     Method called every time an Acquire or WaitAsync call is made to determine what rate limiter to apply to the
	///     request.
	/// </param>
	internal RateLimiterOptions AddPolicy<TPartitionKey>(
		String policyName,
		Func<HttpContext, RateLimitPartition<TPartitionKey>> partitioner)
	{
		if (_policyMap.ContainsKey(policyName))
			throw new InvalidOperationException($"There already exists a policy with the name {policyName}.");

		_policyMap.Add(policyName, new RateLimiterPolicy(ConvertPartitioner(policyName, partitioner), null));

		return this;
	}

	internal RateLimiterOptions SetGlobalPolicy<TPartitionKey>(
		Func<HttpContext, RateLimitPartition<TPartitionKey>> partitioner)
	{
		GlobalPolicy = new RateLimiterPolicy(ConvertPartitioner(nameof(GlobalPolicy), partitioner), null);
		return this;
	}

	private static Func<HttpContext, RateLimitPartition<DefaultKeyType>> ConvertPartitioner<TPartitionKey>(
		String? policyName,
		Func<HttpContext, RateLimitPartition<TPartitionKey>> partitioner)
	{
		return context =>
		{
			var partition = partitioner(context);
			var partitionKey = new DefaultKeyType(policyName, partition.PartitionKey, partition.Factory);
			return new RateLimitPartition<DefaultKeyType>(partitionKey,
				static key => ((Func<TPartitionKey, RateLimiter>)key.Factory!)((TPartitionKey)key.Key!));
		};
	}
}
