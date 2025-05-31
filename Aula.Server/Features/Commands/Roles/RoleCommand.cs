using Aula.Server.Features.Commands.Help;
using Aula.Server.Shared.Cli;

namespace Aula.Server.Features.Commands.Roles;

internal sealed class RoleCommand : Command
{
	public RoleCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		AddSubCommand<CreateRoleSubCommand>();
		AddSubCommand<SetRolePermissionsSubCommand>();
		AddSubCommand<DeleteRoleSubCommand>();
	}

	internal override String Name => "role";

	internal override String Description => "Shows a list of role related commands.";

	internal override async ValueTask Callback(
		IReadOnlyDictionary<String, String> args,
		CancellationToken cancellationToken)
	{
		var helpCommand = ServiceProvider.GetRequiredService<HelpCommand>();
		await helpCommand.Callback(Name, cancellationToken);
	}
}
