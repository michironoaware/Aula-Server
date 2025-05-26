using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Shared.Tokens;

internal interface ITokenProvider
{
	String CreateToken(String id, String stamp);

	Boolean TryReadFromToken(
		ReadOnlySpan<Char> token,
		[NotNullWhen(true)] out String? id,
		[NotNullWhen(true)] out String? stamp);
}
