namespace Aula.Server.Features.Gateway.ConnectToGateway;

internal static class DependencyInjection
{
	internal static IServiceCollection AddConnectToGatewayRequiredServices(this IServiceCollection services)
	{
		GatewayRateLimitingPolicy.Add(services);
		return services;
	}
}
