using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.Cli;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Commands.Roles;

[CommandLineIgnore]
internal sealed partial class SetRolePermissionsSubCommand : Command
{
	private readonly CommandOption _permissionsOption = new()
	{
		Name = "p",
		Description = "The permission flags to set.",
		IsRequired = true,
		RequiresArgument = true,
		CanOverflow = false,
	};

	private readonly CommandOption _roleIdOption = new()
	{
		Name = "r",
		Description = "The id of the role to set the permissions for.",
		IsRequired = true,
		RequiresArgument = true,
		CanOverflow = false,
	};

	public SetRolePermissionsSubCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		AddOptions(_roleIdOption, _permissionsOption);
	}

	internal override String Name => "set-permissions";

	internal override String Description => "Overwrites the permissions of a role with the provided ones.";

	internal override async ValueTask Callback(
		IReadOnlyDictionary<String, String> args,
		CancellationToken cancellationToken)
	{
		var logger = ServiceProvider.GetRequiredService<ILogger<SetRolePermissionsSubCommand>>();

		var roleIdArgument = args[_roleIdOption.Name];
		if (!Snowflake.TryParse(roleIdArgument, out var roleId))
		{
			logger.CommandFailed("The role id must be numeric.");
			return;
		}

		using var serviceScope = ServiceProvider.CreateScope();
		var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

		var role = await dbContext.Roles
			.Where(r => r.Id == roleId && !r.IsRemoved)
			.FirstOrDefaultAsync(cancellationToken);
		if (role is null)
		{
			logger.CommandFailed("The role was not found.");
			return;
		}

		var permissionsArgument = args[_permissionsOption.Name];
		if (!Enum.TryParse(permissionsArgument, true, out Domain.AccessControl.Permissions permissions))
		{
			logger.CommandFailed("Invalid permission flag value or format.");
			return;
		}

		role.Permissions = permissions;
		role.ConcurrencyStamp = Guid.NewGuid().ToString("N");

		try
		{
			_ = await dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateConcurrencyException)
		{
			logger.CommandFailed("Another task was working on the role while updating, try again.");
			return;
		}

		var publisher = serviceScope.ServiceProvider.GetRequiredService<IPublisher>();
		await publisher.Publish(new RoleUpdatedEvent(role), CancellationToken.None);

		LogRoleUpdated(logger);
	}

	[LoggerMessage(LogLevel.Information, "Role permissions updated successfully.")]
	private static partial void LogRoleUpdated(ILogger logger);
}
