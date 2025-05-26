namespace Aula.Server.Features.Files.GetFileContent;

internal static class DependencyInjection
{
	internal static IServiceCollection AddFileContentRequiredServices(this IServiceCollection services)
	{
		_ = services.AddSingleton<FileContentCache>();
		return services;
	}
}
