using Aula.Server.Domain.Bans;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Shared.Authorization;

/// <summary>
///     Denies access banned users.
/// </summary>
internal sealed class BanHandler : AuthorizationHandler<BanRequirement>
{
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		BanRequirement requirement)
	{
		if (context.Resource is not HttpContext httpContext)
			return;

		var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();
		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
			return;

		var dbContext = httpContext.RequestServices.GetRequiredService<AppDbContext>();
		if (await dbContext.Bans
			    .OfType<UserBan>()
			    .AnyAsync(b => b.TargetUserId == user.Id && !b.IsLifted))
		{
			context.Fail();
			return;
		}

		context.Succeed(requirement);
	}
}
