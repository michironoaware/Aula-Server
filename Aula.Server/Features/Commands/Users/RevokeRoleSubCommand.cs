using Aula.Server.Domain.Users;
using Aula.Server.Shared.Cli;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Commands.Users;

[CommandLineIgnore]
internal sealed partial class RevokeRoleSubCommand : Command
{
	private readonly CommandOption _roleIdOption = new()
	{
		Name = "r",
		Description = "The id of the role to revoke.",
		IsRequired = true,
		RequiresArgument = true,
		CanOverflow = false,
	};

	private readonly CommandOption _userIdOption = new()
	{
		Name = "u",
		Description = "The id of the user.",
		IsRequired = true,
		RequiresArgument = true,
		CanOverflow = false,
	};

	public RevokeRoleSubCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{ }

	internal override String Name => "revoke";

	internal override String Description => "Revoke a role to a user.";

	internal override async ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken ct)
	{
		var logger = ServiceProvider.GetRequiredService<ILogger<RevokeRoleSubCommand>>();

		var userIdArgument = args[_userIdOption.Name];
		if (!Snowflake.TryParse(userIdArgument, out var userId))
		{
			logger.CommandFailed("The user id must be numeric.");
			return;
		}

		var roleIdArgument = args[_roleIdOption.Name];
		if (!Snowflake.TryParse(roleIdArgument, out var roleId))
		{
			logger.CommandFailed("The role id must be numeric.");
			return;
		}

		using var serviceScope = ServiceProvider.CreateScope();
		var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

		var user = await dbContext.Users
			.Where(u => u.Id == userId && !u.IsDeleted)
			.FirstOrDefaultAsync(ct);
		if (user is null)
		{
			logger.CommandFailed("The user was not found.");
			return;
		}

		var role = await dbContext.Roles
			.Where(r => r.Id == roleId && !r.IsRemoved)
			.Select(r => new { r.Name })
			.FirstOrDefaultAsync(ct);
		if (role is null)
		{
			logger.CommandFailed("The role was not found.");
			return;
		}

		var roleAssignment = user.RoleAssignments.FirstOrDefault(ra => ra.RoleId == roleId);
		if (roleAssignment is null)
		{
			logger.CommandFailed("The user does not have that role.");
			return;
		}

		_ = user.RoleAssignments.Remove(roleAssignment);
		_ = await dbContext.SaveChangesAsync(ct);

		var publisher = serviceScope.ServiceProvider.GetRequiredService<IPublisher>();
		await publisher.Publish(new UserUpdatedEvent(user), CancellationToken.None);

		LogRoleRevoked(logger, user.DisplayName, role.Name);
	}

	[LoggerMessage(LogLevel.Information, "Role \"{roleName}\" revoked from user \"{userName}\".")]
	private static partial void LogRoleRevoked(ILogger logger, String userName, String roleName);
}
