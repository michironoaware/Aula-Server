using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;

namespace Aula.Server.Shared.Gateway;

internal static class DependencyInjection
{
	internal static IServiceCollection AddGateway(this IServiceCollection services)
	{
		_ = services.AddOptions<GatewayOptions>()
			.BindConfiguration(GatewayOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.TryAddScoped<GatewayManager>();
		_ = services.AddHostedService<ExpiredSessionsCleanerService>();

		_ = services.AddWebSockets(options =>
		{
			options.KeepAliveInterval = TimeSpan.FromSeconds(60);
			options.KeepAliveTimeout = TimeSpan.FromSeconds(48);
		});

		return services;
	}

	internal static TBuilder UseWebSocketHeaderParsing<TBuilder>(this TBuilder builder)
		where TBuilder : IApplicationBuilder
	{
		_ = builder.UseMiddleware<HeaderParsingMiddleware>();
		return builder;
	}
}
