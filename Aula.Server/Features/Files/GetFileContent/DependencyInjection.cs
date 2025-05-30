namespace Aula.Server.Features.Files.GetFileContent;

internal static class DependencyInjection
{
	internal static IServiceCollection AddFileContentRequiredServices(this IServiceCollection services)
	{
		services.TryAddScoped<FileContentCache>();
		return services;
	}
}
