using Aula.Server.Domain.Users;
using Aula.Server.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Aula.Server.Shared.Authorization;

/// <summary>
///     Require users to have their email confirmed.
/// </summary>
internal sealed class EmailConfirmedHandler : AuthorizationHandler<ConfirmedEmailRequirement>
{
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		ConfirmedEmailRequirement requirement)
	{
		if (context.Resource is not HttpContext httpContext)
			return;

		var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();
		if (await userManager.GetUserAsync(httpContext.User) is not StandardUser user)
			return;

		if (user.EmailConfirmed || !userManager.Options.SignIn.RequireConfirmedEmail)
			context.Succeed(requirement);
	}
}
