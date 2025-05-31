using Aula.Server.Features.Commands.Help;
using Aula.Server.Shared.Cli;

namespace Aula.Server.Features.Commands.Permissions;

internal sealed class PermissionsSubCommand : Command
{
	public PermissionsSubCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		AddSubCommand<ListPermissionsSubCommand>();
	}

	internal override String Name => "permissions";

	internal override String Description => "Shows a list of user permissions related commands.";

	internal override async ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken ct)
	{
		var helpCommand = ServiceProvider.GetRequiredService<HelpCommand>();
		await helpCommand.Callback(Name, ct);
	}
}
