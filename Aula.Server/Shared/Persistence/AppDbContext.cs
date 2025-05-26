using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Files;
using Aula.Server.Domain.Messages;
using Aula.Server.Domain.Rooms;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Resilience;
using Aula.Server.Shared.Snowflakes;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Aula.Server.Shared.Persistence;

internal sealed class AppDbContext : DbContext
{
	private readonly ResiliencePipeline _retryOnDbConcurrencyProblem;

	public AppDbContext(
		DbContextOptions<AppDbContext> options,
		[FromKeyedServices(ResiliencePipelines.RetryOnDbConcurrencyProblem)]
		ResiliencePipeline retryOnDbConcurrencyProblem)
		: base(options)
	{
		_retryOnDbConcurrencyProblem = retryOnDbConcurrencyProblem;
	}

	internal DbSet<User> Users => Set<User>();

	internal DbSet<Role> Roles => Set<Role>();

	internal DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();

	internal DbSet<Room> Rooms => Set<Room>();

	internal DbSet<RoomConnection> RoomConnections => Set<RoomConnection>();

	internal DbSet<Message> Messages => Set<Message>();

	internal DbSet<Ban> Bans => Set<Ban>();

	internal DbSet<File> Files => Set<File>();

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		_ = configurationBuilder
			.Properties<Snowflake>()
			.HaveConversion<SnowflakeToUInt64Converter>();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		_ = modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
		modelBuilder.MapDomainEntities();
	}

	public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var entriesWritten = await base.SaveChangesAsync(cancellationToken);
		ChangeTracker.Clear();
		return entriesWritten;
	}

	public async Task<Int32> SaveChangesWithConcurrencyByPassAsync(CancellationToken cancellationToken = default)
	{
		var saved = false;
		var entriesWritten = 0;
		while (!saved)
		{
			try
			{
				entriesWritten = await SaveChangesAsync(cancellationToken);
				saved = true;
			}
			catch (DbUpdateConcurrencyException ex)
			{
				foreach (var entry in ex.Entries)
				{
					var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
					if (databaseValues is null)
						throw;

					// Refresh original values to bypass next concurrency check
					entry.OriginalValues.SetValues(databaseValues);
				}
			}
		}

		ChangeTracker.Clear();
		return entriesWritten;
	}
}
