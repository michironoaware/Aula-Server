namespace Aula.Server.Features.Identity.ConfirmEmail;

internal static class DependencyInjection
{
	internal static IServiceCollection AddConfirmEmailRequiredServices(this IServiceCollection services)
	{
		_ = services.AddScoped<ConfirmEmailEmailSender>();
		return services;
	}
}
