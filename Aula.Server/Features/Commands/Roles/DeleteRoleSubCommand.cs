using Aula.Server.Domain.AccessControl;
using Aula.Server.Shared.Cli;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Commands.Roles;

[CommandLineIgnore]
internal sealed partial class DeleteRoleSubCommand : Command
{
	private readonly CommandOption _roleIdOption = new()
	{
		Name = "r",
		Description = "The id of the role to delete.",
		IsRequired = true,
		RequiresArgument = true,
		CanOverflow = false,
	};

	public DeleteRoleSubCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		AddOptions(_roleIdOption);
	}

	internal override String Name => "delete";

	internal override String Description => "Deletes a role.";

	internal override async ValueTask Callback(
		IReadOnlyDictionary<String, String> args,
		CancellationToken cancellationToken)
	{
		var logger = ServiceProvider.GetRequiredService<ILogger<DeleteRoleSubCommand>>();

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

		if (role.IsGlobal)
		{
			logger.CommandFailed("Cannot delete a global role.");
			return;
		}

		role.IsRemoved = true;
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
		await publisher.Publish(new RoleDeletedEvent(role), CancellationToken.None);

		LogRoleDeleted(logger, role.Name);
	}

	[LoggerMessage(LogLevel.Information, "Role \"{name}\" deleted successfully.")]
	private static partial void LogRoleDeleted(ILogger logger, String name);
}
