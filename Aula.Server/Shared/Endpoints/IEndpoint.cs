using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Shared.Endpoints;

internal interface IEndpoint
{
	void Build(IEndpointRouteBuilder route);
}
