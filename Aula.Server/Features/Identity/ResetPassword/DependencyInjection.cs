namespace Aula.Server.Features.Identity.ResetPassword;

internal static class DependencyInjection
{
	internal static IServiceCollection AddResetPasswordRequiredServices(this IServiceCollection services)
	{
		_ = services.AddScoped<ResetPasswordEmailSender>();
		return services;
	}
}
