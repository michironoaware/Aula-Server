using Aula.Server.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Features.Ping;

internal sealed class PingEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("ping", Handle)
			.HasApiVersion(1);
	}

	private static Ok Handle() => TypedResults.Ok();
}
