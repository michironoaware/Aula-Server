using Aula.Server.Domain.AccessControl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace Aula.Server.Shared.Authorization;

internal static class PermissionsPolicyExtensions
{
	private const String PolicyName = nameof(PermissionsRequirement);

	/// <summary>
	///     Enforces the authenticated user to present one of the specified permissions.
	/// </summary>
	/// <param name="builder">The endpoint convention builder.</param>
	/// <param name="permissions">
	///     The required permissions. The requirement will succeed if the authenticated user have
	///     at least one of these, or <see cref="Permissions.Administrator" />.
	/// </param>
	/// <returns>The original convention builder parameter.</returns>
	internal static TBuilder RequirePermissions<TBuilder>(
		this TBuilder builder,
		params IEnumerable<Permissions> permissions)
		where TBuilder : IEndpointConventionBuilder =>
		builder
			.RequireAuthorization(PolicyName)
			.WithMetadata(new RequirePermissionsAttribute([ Permissions.Administrator, ..permissions ]));

	/// <summary>
	///     Adds a policy that requires users to have specific permissions.
	/// </summary>
	/// <param name="builder">The authorization builder.</param>
	/// <returns>The original authorization builder.</returns>
	internal static AuthorizationBuilder AddPermissionsPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy => policy.AddRequirements(new PermissionsRequirement()));
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, PermissionsHandler>());
		return builder;
	}
}
