namespace Aula.Server.Features.Messages.DeleteMessage;

internal static class DependencyInjection
{
	internal static IServiceCollection AddRemoveMessageRequiredServices(this IServiceCollection services)
	{
		DeleteMessageRateLimitingPolicy.Add(services);
		return services;
	}
}
