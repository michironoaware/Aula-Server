using Aula.Server.Shared.Cli;
using Aula.Server.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Commands.Database;

[CommandLineIgnore]
internal sealed partial class ApplyMigrationsSubCommand : Command
{
	private static readonly CommandOption s_forceOption = new()
	{
		Name = "force",
		Description = "Forces the database to migrate migrations.",
		IsRequired = false,
		RequiresArgument = false,
		CanOverflow = false,
	};

	public ApplyMigrationsSubCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		AddOptions(s_forceOption);
	}

	internal override String Name => "apply-migrations";

	internal override String Description => "Applies database migrations.";

	internal override async ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var logger = ServiceProvider.GetRequiredService<ILogger<ApplyMigrationsSubCommand>>();
		LogConsequences(logger);

		var force = args.ContainsKey(s_forceOption.Name);
		if (!force)
		{
			LogForceFlagRequirement(logger);
			return;
		}

		LogMigrationsStarted(logger);
		var dbContext = ServiceProvider.GetRequiredService<AppDbContext>();
		await dbContext.Database.MigrateAsync(ct);
		LogMigrationsApplied(logger);
	}

	[LoggerMessage(
		LogLevel.Warning,
		"Running without applying migrations may cause runtime errors, data mismatches," +
		"or system failures if the database schema is out of sync with the application." +
		"However, applying migrations modifies the database schema and" +
		"may result in data loss or downtime if not reviewed carefully.")]
	private static partial void LogConsequences(ILogger logger);

	[LoggerMessage(LogLevel.Information, "Database migrations were not applied. Use the '-force' flag to apply them.")]
	private static partial void LogForceFlagRequirement(ILogger logger);

	[LoggerMessage(LogLevel.Information, "Applying database migrations...")]
	private static partial void LogMigrationsStarted(ILogger logger);

	[LoggerMessage(LogLevel.Information, "Database migrations applied successfully.")]
	private static partial void LogMigrationsApplied(ILogger logger);
}
