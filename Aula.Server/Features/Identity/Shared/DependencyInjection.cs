namespace Aula.Server.Features.Identity.Shared;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentitySharedServices(this IServiceCollection services)
	{
		_ = services.AddOptions<IdentityFeatureOptions>()
			.BindConfiguration(IdentityFeatureOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();
		return services;
	}
}
