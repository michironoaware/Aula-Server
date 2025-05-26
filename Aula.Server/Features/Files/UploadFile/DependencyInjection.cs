namespace Aula.Server.Features.Files.UploadFile;

internal static class DependencyInjection
{
	internal static IServiceCollection AddFileFeaturesSharedServices(this IServiceCollection services)
	{
		_ = services.AddOptions<UploadFileOptions>()
			.BindConfiguration($"RateLimiting:{nameof(UploadFileOptions.SectionName)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();
		_ = FileRateLimitingPolicy.Add(services);
		return services;
	}
}
