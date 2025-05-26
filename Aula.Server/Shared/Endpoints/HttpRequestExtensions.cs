using Microsoft.AspNetCore.Http;

namespace Aula.Server.Shared.Endpoints;

internal static class HttpRequestExtensions
{
	private const String SchemeDelimiter = "://";

	internal static String GetUrl(this HttpRequest request)
	{
		var scheme = request.Scheme;
		var host = request.Host.Value ?? String.Empty;
		var pathBase = request.PathBase.Value ?? String.Empty;
		var path = request.Path.Value ?? String.Empty;
		return $"{scheme}{SchemeDelimiter}{host}{pathBase}{path}";
	}

	internal static String GetHost(this HttpRequest request)
	{
		var scheme = request.Scheme;
		var host = request.Host.Value ?? String.Empty;
		return $"{scheme}{SchemeDelimiter}{host}";
	}
}
