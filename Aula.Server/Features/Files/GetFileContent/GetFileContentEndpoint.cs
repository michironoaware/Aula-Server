using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Snowflakes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Features.Files.GetFileContent;

internal sealed class GetFileContentEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("files/{fileId}/content", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<FileContentHttpResult, NotFound>> HandleAsync(
		[FromRoute] Snowflake fileId,
		[FromServices] FileContentCache fileContentCache,
		CancellationToken ct)
	{
		var fileContent = await fileContentCache.GetByIdAsync(fileId, ct);
		if (fileContent is null)
			return TypedResults.NotFound();
		return TypedResults.File(fileContent);
	}
}
