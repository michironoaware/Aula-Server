using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.Cli;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;

namespace Aula.Server.Features.Commands.Roles;

[CommandLineIgnore]
internal sealed partial class CreateRoleSubCommand : Command
{
	private static readonly CommandOption s_permissionsOption = new()
	{
		Name = "p",
		Description = "The permission flags for the role.",
		IsRequired = true,
		RequiresArgument = true,
		CanOverflow = false,
	};

	public CreateRoleSubCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{ }

	internal override String Name => "create";

	internal override String Description => "Creates a new role.";

	internal override async ValueTask Callback(
		IReadOnlyDictionary<String, String> args,
		CancellationToken cancellationToken)
	{
		var logger = ServiceProvider.GetRequiredService<ILogger<CreateRoleSubCommand>>();

		var permissionsArgument = args[s_permissionsOption.Name];
		if (!Enum.TryParse(permissionsArgument, true, out Domain.AccessControl.Permissions permissions))
		{
			logger.CommandFailed("Invalid permission flag value or format.");
			return;
		}

		using var serviceScope = ServiceProvider.CreateScope();
		var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
		var snowflakes = serviceScope.ServiceProvider.GetRequiredService<ISnowflakeGenerator>();

		var id = await snowflakes.GenerateAsync();
		var role = new Role(id, "New role", permissions, 1, false);
		_ = dbContext.Roles.Add(role);
		_ = await dbContext.SaveChangesAsync(cancellationToken);

		var publisher = serviceScope.ServiceProvider.GetRequiredService<IPublisher>();
		await publisher.Publish(new RoleCreatedEvent(role), CancellationToken.None);

		LogRoleCreated(logger, id);
	}

	[LoggerMessage(LogLevel.Information, "Role created with ID '{id}'")]
	private static partial void LogRoleCreated(ILogger logger, Snowflake id);
}
