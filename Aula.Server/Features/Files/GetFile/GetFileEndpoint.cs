using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Files.GetFile;

internal sealed class GetFileEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("files/{fileId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<FileData>, NotFound>> HandleAsync(
		[FromRoute] Snowflake fileId,
		[FromServices] AppDbContext dbContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var file = await dbContext.Files
			.AsNoTracking()
			.Where(f => f.Id == fileId)
			.Select(f => new FileData
			{
				Id = f.Id, Name = f.Name, ContentType = f.ContentType, Size = f.Size,
			})
			.FirstOrDefaultAsync(ct);

		if (file is null)
			return TypedResults.NotFound();

		return TypedResults.Ok(file);
	}
}
