using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace Aula.Server.Shared.Authorization;

internal static class UserTypePolicyExtensions
{
	private const String PolicyName = nameof(UserTypeRequirement);

	/// <summary>
	///     Enforces the authenticated user to be of one of the specified types.
	/// </summary>
	/// <param name="builder">The endpoint convention builder.</param>
	/// <param name="authorizedTypes">
	///     The permitted user types. The requirement will succeed if the authenticated user is one
	///     of these.
	/// </param>
	/// <returns>The original convention builder parameter.</returns>
	internal static TBuilder RequireUserType<TBuilder>(
		this TBuilder builder,
		params IEnumerable<UserType> authorizedTypes)
		where TBuilder : IEndpointConventionBuilder
	{
		_ = builder
			.RequireAuthorization(PolicyName)
			.WithMetadata(new RequireUserTypeAttribute([ ..authorizedTypes ]));
		return builder;
	}

	/// <summary>
	///     Adds a policy that requires users to be of a specific type.
	/// </summary>
	/// <param name="builder">The authorization builder.</param>
	/// <returns>The original authorization builder.</returns>
	internal static AuthorizationBuilder AddUserTypePolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy => policy.AddRequirements(new UserTypeRequirement()));
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, UserTypeHandler>());
		return builder;
	}
}
