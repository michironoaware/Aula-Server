using Aula.Server.Domain.Users;
using Aula.Server.Shared.Tokens;

namespace Aula.Server.Shared.Identity;

internal static class TokenProviderExtensions
{
	internal static String CreateToken(this ITokenProvider tokenProvider, User user)
	{
		var id = user.Id.ToString();
		var stamp = user.SecurityStamp ?? throw new ArgumentException("The user security stamp cannot be null.");
		return tokenProvider.CreateToken(id, stamp);
	}
}
