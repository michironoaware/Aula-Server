using System.ComponentModel.DataAnnotations;
using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Users;
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
				.SeedGlobalRole(serviceProvider)
				.SeedAdminUser(serviceProvider);

			_ = settings.Provider switch
			{
				PersistenceProvider.Sqlite => builder.UseSqlite(settings.ConnectionString,
					o => o.UseQuerySplittingBehavior(settings.QuerySplittingBehavior)),
				PersistenceProvider.Postgres => builder.UseNpgsql(settings.ConnectionString,
					o => o.UseQuerySplittingBehavior(settings.QuerySplittingBehavior)
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

	private static TBuilder SeedAdminUser<TBuilder>(
		this TBuilder builder,
		IServiceProvider serviceProvider)
		where TBuilder : DbContextOptionsBuilder
	{
		var snowflakes = serviceProvider.GetRequiredService<ISnowflakeGenerator>();

		_ = builder.UseSeeding((dbContext, _1) =>
			{
				var adminRole = dbContext.Set<Role>()
					.FirstOrDefault(r => r.Permissions.HasFlag(Permissions.Administrator) && !r.IsRemoved);
				if (adminRole is null)
				{
					adminRole = new Role(snowflakes.Generate(), "Administrator", Permissions.Administrator, 1, false);
					_ = dbContext.Set<Role>().Add(adminRole);
					_ = dbContext.SaveChanges();
				}

				var adminUser = dbContext.Set<User>()
					.OfType<BotUser>()
					.FirstOrDefault(u => u.RoleAssignments.Any(r => r.RoleId == adminRole.Id) && !u.IsDeleted);
				if (adminUser is null)
				{
					adminUser = new BotUser(snowflakes.Generate(), "Administrator", String.Empty)
					{
						SecurityStamp = Guid.NewGuid().ToString("N"),
					};
					_ = dbContext.Set<User>().Add(adminUser);
					_ = dbContext.SaveChanges();
				}

				if (adminUser.RoleAssignments.All(r => r.RoleId != adminRole.Id))
				{
					var roleAssignment = new RoleAssignment(snowflakes.Generate(), adminRole.Id, adminUser.Id);
					adminUser.RoleAssignments.Add(roleAssignment);
					_ = dbContext.SaveChanges();
				}
			})
			.UseAsyncSeeding(async (dbContext, _1, ct) =>
			{
				var adminRole = await dbContext.Set<Role>()
					.FirstOrDefaultAsync(r => r.Permissions.HasFlag(Permissions.Administrator) && !r.IsRemoved, ct);
				if (adminRole is null)
				{
					adminRole = new Role(await snowflakes.GenerateAsync(), "Administrator", Permissions.Administrator,
						1, false);
					_ = dbContext.Set<Role>().Add(adminRole);
					_ = await dbContext.SaveChangesAsync(ct);
				}

				var adminUser = await dbContext.Set<User>()
					.OfType<BotUser>()
					.FirstOrDefaultAsync(u => u.RoleAssignments.Any(r => r.RoleId == adminRole.Id) && !u.IsDeleted, ct);
				if (adminUser is null)
				{
					adminUser = new BotUser(await snowflakes.GenerateAsync(), "Administrator", String.Empty)
					{
						SecurityStamp = Guid.NewGuid().ToString("N"),
					};
					_ = dbContext.Set<User>().Add(adminUser);
					_ = await dbContext.SaveChangesAsync(ct);
				}

				if (adminUser.RoleAssignments.All(r => r.RoleId != adminRole.Id))
				{
					var assignment = new RoleAssignment(await snowflakes.GenerateAsync(), adminRole.Id, adminUser.Id);
					adminUser.RoleAssignments.Add(assignment);
					_ = await dbContext.SaveChangesAsync(ct);
				}
			});

		return builder;
	}
}
