using System.Diagnostics;
using System.Threading.RateLimiting;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.RateLimiting;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Messages.RemoveMessage;

internal static class RemoveMessageRateLimitingPolicy
{
	internal const String Name = nameof(RemoveMessageRateLimitingPolicy);

	internal static void Add(IServiceCollection services)
	{
		_ = services.AddOptions<RateLimitOptions>(Name)
			.BindConfiguration($"RateLimiting:{nameof(RemoveMessage)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(Name, options =>
		{
			options.WindowMilliseconds ??= 60000;
			options.PermitLimit ??= 30;
		});

		_ = services.AddCustomRateLimiter(options =>
		{
			_ = options.AddPolicy(Name, httpContext =>
			{
				if (!httpContext.Request.RouteValues.TryGetValue("roomId", out var roomId))
					throw new UnreachableException("roomId not found. Maybe the route value name has changed.");

				var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();
				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey =
					$"{userId?.ToString() ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty}.{roomId}";

				var rateLimit = httpContext.RequestServices
					.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
					.Get(Name);

				return RateLimitPartitionExtensions.GetExtendedFixedWindowRateLimiter(partitionKey,
					_ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = rateLimit.PermitLimit!.Value,
						Window = TimeSpan.FromMilliseconds(rateLimit.WindowMilliseconds!.Value),
					});
			});
		});
	}
}
