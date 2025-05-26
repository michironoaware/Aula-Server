using System.Threading.RateLimiting;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.RateLimiting;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Gateway.ConnectToGateway;

internal static class GatewayRateLimitingPolicy
{
	internal const String Name = nameof(GatewayRateLimitingPolicy);

	internal static void Add(IServiceCollection services)
	{
		_ = services.AddOptions<RateLimitOptions>(Name)
			.BindConfiguration($"RateLimiters:{nameof(ConnectToGateway)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(Name, options =>
		{
			options.WindowMilliseconds ??= (Int32)TimeSpan.FromHours(24).TotalMilliseconds;
			options.PermitLimit ??= 1000;
		});

		_ = services.AddCustomRateLimiter(options =>
		{
			_ = options.AddPolicy(Name, httpContext =>
			{
				var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId?.ToString() ??
					httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;

				var rateLimitOptions = httpContext.RequestServices
					.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
					.Get(Name);

				return RateLimitPartitionExtensions.GetExtendedFixedWindowRateLimiter(partitionKey,
					_ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = rateLimitOptions.PermitLimit!.Value,
						Window = TimeSpan.FromMilliseconds(rateLimitOptions.WindowMilliseconds!.Value),
					});
			});
		});
	}
}
