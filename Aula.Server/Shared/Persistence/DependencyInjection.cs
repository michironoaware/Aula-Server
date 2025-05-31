using System.ComponentModel.DataAnnotations;
using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.Snowflakes;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Aula.Server.Shared.Persistence;

internal static class DependencyInjection
{
	internal static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
	{
		_ = services.AddOptions<PersistenceOptions>()
			.BindConfiguration(PersistenceOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		var settings = new PersistenceOptions();
		configuration.GetSection(PersistenceOptions.SectionName).Bind(settings);
		Validator.ValidateObject(settings, new ValidationContext(settings));

		_ = services.AddDbContext<AppDbContext>((serviceProvider, builder) =>
		{
			_ = builder
				.UseLazyLoadingProxies()
				.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
				.SeedGlobalRole(serviceProvider);

			_ = settings.Provider switch
			{
				PersistenceProvider.Sqlite => builder.UseSqlite(settings.ConnectionString,
					o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)),
				PersistenceProvider.Postgres => builder.UseNpgsql(settings.ConnectionString,
					o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
						.SetPostgresVersion(settings.PostgresVersionMajor, settings.PostgresVersionMinor)),
				_ => throw new NotImplementedException(),
			};
		});

		return services;
	}

	internal static TBuilder HandleDbUpdateConcurrencyExceptions<TBuilder>(this TBuilder builder)
		where TBuilder : IApplicationBuilder
	{
		_ = builder.UseMiddleware<DbUpdateConcurrencyExceptionHandlingMiddleware>();
		return builder;
	}

	private static TBuilder SeedGlobalRole<TBuilder>(
		this TBuilder builder,
		IServiceProvider serviceProvider)
		where TBuilder : DbContextOptionsBuilder
	{
		var snowflakes = serviceProvider.GetRequiredService<ISnowflakeGenerator>();

		_ = builder.UseSeeding((dbContext, _1) =>
			{
				var globalRole = dbContext.Set<Role>().FirstOrDefault(r => r.IsGlobal);
				if (globalRole is null)
				{
					globalRole = new Role(snowflakes.Generate(), "Global", Permissions.None, 0, true);
					_ = dbContext.Set<Role>().Add(globalRole);
					_ = dbContext.SaveChanges();
				}
			})
			.UseAsyncSeeding(async (dbContext, _1, ct) =>
			{
				ct.ThrowIfCancellationRequested();
				var globalRole = await dbContext.Set<Role>().FirstOrDefaultAsync(r => r.IsGlobal, ct);
				if (globalRole is null)
				{
					globalRole = new Role(await snowflakes.GenerateAsync(), "Global", Permissions.None, 0, true);
					_ = dbContext.Set<Role>().Add(globalRole);
					_ = await dbContext.SaveChangesAsync(ct);
				}
			});

		return builder;
	}
}
