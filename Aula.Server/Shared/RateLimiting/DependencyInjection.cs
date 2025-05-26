using System.Diagnostics;
using System.Threading.RateLimiting;
using Aula.Server.Shared.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Aula.Server.Shared.RateLimiting;

internal static class DependencyInjection
{
	private const String GlobalPolicyName = "Global";

	internal static IServiceCollection AddCustomRateLimiter(
		this IServiceCollection services,
		Action<RateLimiterOptions> configureOptions)
	{
		_ = services.AddCustomRateLimiter();
		_ = services.Configure(configureOptions);
		return services;
	}

	internal static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
	{
		services.TryAddSingleton<RateLimiterManager>();
		_ = services.AddHostedService<IdleRateLimitersCleanerService>();
		_ = services.AddHostedService<ClearOnConfigurationUpdateService>();

		_ = services.AddOptions<RateLimitOptions>(GlobalPolicyName)
			.BindConfiguration("RateLimiting:Global")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(GlobalPolicyName, options =>
		{
			options.WindowMilliseconds ??= 1000;
			options.PermitLimit ??= 30;
		});

		_ = services.Configure<RateLimiterOptions>(options =>
		{
			if (options.GlobalPolicy is not null)
				return;

			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			_ = options.SetGlobalPolicy(httpContext =>
			{
				var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId.HasValue
					? userId.Value.ToString()
					: httpContext.Connection.RemoteIpAddress?.ToString();
				if (partitionKey is null ||
				    partitionKey.Length == 0)
					throw new NotImplementedException("Fallback not implemented.");

				return RateLimitPartitionExtensions.GetExtendedFixedWindowRateLimiter(partitionKey, _ =>
				{
					var rateLimit = httpContext.RequestServices
						.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
						.Get(GlobalPolicyName);

					return new FixedWindowRateLimiterOptions
					{
						PermitLimit = rateLimit.PermitLimit!.Value,
						Window = TimeSpan.FromMilliseconds(rateLimit.WindowMilliseconds!.Value),
					};
				});
			});
		});

		return services;
	}

	internal static TBuilder UseCustomRateLimiting<TBuilder>(this TBuilder builder)
		where TBuilder : IApplicationBuilder
	{
		_ = builder.Use((httpContext, next) =>
		{
			var endpoint = httpContext.GetEndpoint();
			if (endpoint is null)
				return next(httpContext);

			var ignoreRateLimitingAttribute = endpoint.Metadata.GetMetadata<IgnoreRateLimitingAttribute>();
			if (ignoreRateLimitingAttribute is not null)
				return next(httpContext);

			var rateLimiterManager = httpContext.RequestServices.GetRequiredService<RateLimiterManager>();
			var rateLimiterOptions =
				httpContext.RequestServices.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

			var globalRateLimitOptions = httpContext.RequestServices
				.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
				.Get(GlobalPolicyName);
			httpContext.Response.Headers.Append("X-RateLimit-Global-Limit",
				globalRateLimitOptions.PermitLimit.ToString());
			httpContext.Response.Headers.Append("X-RateLimit-Global-WindowMilliseconds",
				globalRateLimitOptions.WindowMilliseconds.ToString());

			var globalRateLimiter =
				rateLimiterManager.GetOrAdd(rateLimiterOptions.GlobalPolicy!.GetPartition(httpContext));
			var globalLease = globalRateLimiter.AttemptAcquire();
			var globalStatistics = globalRateLimiter.GetStatistics() ?? throw new UnreachableException();

			if (globalStatistics.CurrentAvailablePermits is 0)
			{
				httpContext.Response.StatusCode = rateLimiterOptions.RejectionStatusCode;
				httpContext.Response.Headers.Append("X-RateLimit-IsGlobal", "true");
				var replenishmentDateTime =
					((ExtendedReplenishingRateLimiter)globalRateLimiter).ReplenishmentDate!.Value;
				httpContext.Response.Headers.Append("X-RateLimit-ResetsAt", replenishmentDateTime.ToString("O"));
			}

			var rateLimit = endpoint.Metadata.GetMetadata<RequireRateLimitingAttribute>();
			if (rateLimit is null)
			{
				if (globalLease.IsAcquired)
					return next(httpContext);

				return Task.CompletedTask;
			}

			// We assume that the rate limit configuration will have the same name as the policy
			var rateLimitOptions = httpContext.RequestServices
				.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
				.Get(rateLimit.PolicyName);
			httpContext.Response.Headers.Append("X-RateLimit-Route-Limit", rateLimitOptions.PermitLimit.ToString());
			httpContext.Response.Headers.Append("X-RateLimit-Route-WindowMilliseconds",
				rateLimitOptions.WindowMilliseconds.ToString());

			if (!globalLease.IsAcquired)
				return Task.CompletedTask;

			if (!rateLimiterOptions.PolicyMap.TryGetValue(rateLimit.PolicyName, out var policy))
			{
				throw new InvalidOperationException(
					$"This endpoint requires a rate limiting policy with name {rateLimit.PolicyName}, but no such policy exists.");
			}

			var rateLimiter = rateLimiterManager.GetOrAdd(policy.GetPartition(httpContext));
			var lease = rateLimiter.AttemptAcquire();
			var statistics = rateLimiter.GetStatistics() ?? throw new UnreachableException();

			if (statistics.CurrentAvailablePermits is 0 &&
			    rateLimiter is ExtendedReplenishingRateLimiter replenishingRateLimiter)
			{
				var replenishmentDateTime = replenishingRateLimiter.ReplenishmentDate!.Value;
				httpContext.Response.Headers.Append("X-RateLimit-ResetsAt", replenishmentDateTime.ToString("O"));
			}

			if (!lease.IsAcquired)
			{
				httpContext.Response.StatusCode = rateLimiterOptions.RejectionStatusCode;
				httpContext.Response.Headers.Append("X-RateLimit-IsGlobal", "false");
				return Task.CompletedTask;
			}

			return next(httpContext);
		});

		return builder;
	}

	internal static TBuilder IgnoreRateLimiting<TBuilder>(this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.WithMetadata(new IgnoreRateLimitingAttribute());

	internal static TBuilder ApplyRateLimiting<TBuilder>(this TBuilder builder, String policyName)
		where TBuilder : IEndpointConventionBuilder =>
		builder.WithMetadata(new RequireRateLimitingAttribute(policyName));
}
