using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace Aula.Server.Shared.Authorization;

internal static class BanPolicyExtensions
{
	private const String PolicyName = nameof(BanRequirement);

	/// <summary>
	///     Deny banned users from accessing the endpoint.
	/// </summary>
	/// <param name="builder">The endpoint convention builder.</param>
	/// <returns>The original convention builder parameter.</returns>
	internal static TBuilder DenyBannedUsers<TBuilder>(this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.RequireAuthorization(PolicyName);

	/// <summary>
	///     Adds a ban policy that requires users to not be banned.
	/// </summary>
	/// <param name="builder">The authorization builder.</param>
	/// <returns>The original authorization builder.</returns>
	internal static AuthorizationBuilder AddBanPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy => policy.AddRequirements(new BanRequirement()));
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, BanHandler>());
		return builder;
	}
}
