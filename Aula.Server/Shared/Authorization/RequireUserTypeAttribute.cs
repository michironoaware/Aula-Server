using Aula.Server.Domain.Users;

namespace Aula.Server.Shared.Authorization;

/// <summary>
///     Indicates the allowed user types for an endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RequireUserTypeAttribute : Attribute
{
	internal RequireUserTypeAttribute(params IReadOnlyList<UserType> authorizedTypes)
	{
		if (authorizedTypes.Count < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(authorizedTypes),
				$"At least one {nameof(UserType)} must be assigned to the ${nameof(RequireUserTypeAttribute)}");
		}

		AuthorizedTypes = authorizedTypes;
	}

	public IReadOnlyList<UserType> AuthorizedTypes { get; }
}
