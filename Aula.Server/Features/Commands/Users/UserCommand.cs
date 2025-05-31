using Aula.Server.Features.Commands.Help;
using Aula.Server.Shared.Cli;

namespace Aula.Server.Features.Commands.Users;

internal sealed class UserCommand : Command
{
	public UserCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		AddSubCommand<AssignRoleSubCommand>();
		AddSubCommand<RevokeRoleSubCommand>();
	}

	internal override String Name => "user";

	internal override String Description => "Shows a list of user related commands.";

	internal override async ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken ct)
	{
		var helpCommand = ServiceProvider.GetRequiredService<HelpCommand>();
		await helpCommand.Callback(Name, ct);
	}
}
