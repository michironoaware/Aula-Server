using Aula.Server.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Aula.Server.Shared.Authorization;

/// <summary>
///     Denies access to users whose type is not permitted for the endpoint.
/// </summary>
internal sealed class UserTypeHandler : AuthorizationHandler<UserTypeRequirement>
{
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		UserTypeRequirement requirement)
	{
		if (context.Resource is not HttpContext httpContext)
			return;

		var endpoint = httpContext.GetEndpoint();
		var authorizedTypes = endpoint?.Metadata.GetMetadata<RequireUserTypeAttribute>()?.AuthorizedTypes;
		if (authorizedTypes is null)
			return;

		var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();
		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
			return;

		if (authorizedTypes.Any(type => type == user.Type))
			context.Succeed(requirement);
	}
}
