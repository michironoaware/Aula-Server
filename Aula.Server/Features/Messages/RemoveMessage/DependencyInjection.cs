namespace Aula.Server.Features.Messages.RemoveMessage;

internal static class DependencyInjection
{
	internal static IServiceCollection AddRemoveMessageRequiredServices(this IServiceCollection services)
	{
		RemoveMessageRateLimitingPolicy.Add(services);
		return services;
	}
}
