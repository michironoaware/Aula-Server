using System.Threading.RateLimiting;
using Aula.Server.Shared.Identity;
using Microsoft.AspNetCore.Builder;

namespace Aula.Server.Features.Files.UploadFile;

internal static class FileRateLimitingPolicy
{
	internal const String Name = nameof(FileRateLimitingPolicy);

	internal static IServiceCollection Add(IServiceCollection services)
	{
		_ = services.AddRateLimiter(options =>
		{
			_ = options.AddPolicy(Name, httpContext =>
			{
				var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId?.ToString() ??
					httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;

				return RateLimitPartition.GetConcurrencyLimiter(partitionKey,
					_ => new ConcurrencyLimiterOptions
					{
						PermitLimit = 1, QueueLimit = 1, QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
					});
			});
		});

		return services;
	}
}
