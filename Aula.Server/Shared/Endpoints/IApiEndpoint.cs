using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Shared.Endpoints;

internal interface IApiEndpoint
{
	void Build(IEndpointRouteBuilder route);
}
