using Aula.Server.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace Aula.Server.Shared.Authorization;

internal static class AuthenticatedUserPolicyExtensions
{
	private const String PolicyName = "AuthenticatedUserPolicy";

	/// <summary>
	///     Require clients to be authenticated.
	/// </summary>
	/// <param name="builder">The endpoint convention builder.</param>
	/// <returns>The original convention builder parameter.</returns>
	internal static TBuilder RequireAuthenticatedUser<TBuilder>(this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.RequireAuthorization(PolicyName);

	/// <summary>
	///     Adds an authorization policy that requires clients to be authenticated.
	/// </summary>
	/// <param name="builder">The authorization builder.</param>
	/// <returns>The original authorization builder.</returns>
	internal static AuthorizationBuilder AddAuthenticatedUserPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy =>
		{
			_ = policy.RequireAuthenticatedUser();
			_ = policy.AddAuthenticationSchemes(AuthenticationSchemes.BearerToken);
		});

		return builder;
	}
}
