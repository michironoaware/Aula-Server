using System.Text;
using Aula.Server.Shared.Cli;

namespace Aula.Server.Features.Commands.Permissions;

[CommandLineIgnore]
internal sealed partial class ListPermissionsSubCommand : Command
{
	public ListPermissionsSubCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{ }

	internal override String Name => "list";

	internal override String Description =>
		"Shows a list of all the existing permissions and their corresponding values.";

	internal override ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken)
	{
		var logger = ServiceProvider.GetRequiredService<ILogger<ListPermissionsSubCommand>>();
		var permissionsMessage = new StringBuilder(Environment.NewLine);

		foreach (var permission in Enum.GetValues<Domain.AccessControl.Permissions>())
			_ = permissionsMessage.AppendLine($"- {permission}: {(Int32)permission}");

		ExistingPermissions(logger, permissionsMessage.ToString());
		return ValueTask.CompletedTask;
	}

	[LoggerMessage(LogLevel.Information, "Permissions: {permissions}")]
	internal static partial void ExistingPermissions(ILogger logger, String permissions);
}
