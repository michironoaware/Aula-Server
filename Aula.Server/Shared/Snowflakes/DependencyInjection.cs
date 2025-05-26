namespace Aula.Server.Shared.Snowflakes;

internal static class DependencyInjection
{
	internal static IServiceCollection AddSnowflakes(this IServiceCollection services)
	{
		_ = services.AddOptions<SnowflakeOptions>()
			.BindConfiguration(SnowflakeOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();
		_ = services.AddSingleton<ISnowflakeGenerator, DefaultSnowflakeGenerator>();
		return services;
	}
}
