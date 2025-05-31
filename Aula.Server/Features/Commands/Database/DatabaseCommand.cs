using Aula.Server.Features.Commands.Help;
using Aula.Server.Shared.Cli;

namespace Aula.Server.Features.Commands.Database;

internal sealed class DatabaseCommand : Command
{
	public DatabaseCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		AddSubCommand<ApplyMigrationsCommand>();
	}

	internal override String Name => "db";

	internal override String Description => "Shows a list of db related commands.";

	internal override async ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken ct)
	{
		var helpCommand = ServiceProvider.GetRequiredService<HelpCommand>();
		await helpCommand.Callback(Name, ct);
	}
}
