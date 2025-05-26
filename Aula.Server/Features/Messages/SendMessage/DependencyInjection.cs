namespace Aula.Server.Features.Messages.SendMessage;

internal static class DependencyInjection
{
	internal static IServiceCollection AddSendMessageRequiredServices(this IServiceCollection services)
	{
		SendMessageRateLimitingPolicy.Add(services);
		return services;
	}
}
