using Aula.Server.Features.Files.Shared;
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

namespace Aula.Server.Features.Files.GetFiles;

internal sealed class GetFilesEndpoint : IApiEndpoint
{
	internal const String AfterQueryParameter = "after";
	internal const String CountQueryParameter = "count";
	internal const Int32 MinimumMessageCount = 1;
	internal const Int32 MaximumMessageCount = 100;
	internal const Int32 DefaultMessageCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("files", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<FileData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = AfterQueryParameter)] Snowflake? afterFileId,
		[FromQuery(Name = CountQueryParameter)] Int32? specifiedCount,
		[FromServices] AppDbContext dbContext,
		CancellationToken ct)
	{
		var count = specifiedCount ?? DefaultMessageCount;
		if (count is > MaximumMessageCount or < MinimumMessageCount)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidFileCount);

		var filesQuery = dbContext.Files
			.OrderByDescending(f => f.UploadDate)
			.AsQueryable();

		if (afterFileId is not null)
		{
			var targetFile = await dbContext.Files
				.Where(f => f.Id == afterFileId)
				.Select(m => new { m.UploadDate })
				.FirstOrDefaultAsync(ct);
			if (targetFile is null)
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterFile);

			filesQuery = filesQuery.Where(x => x.UploadDate > targetFile.UploadDate);
		}

		var files = await filesQuery
			.Select(f => new FileData
			{
				Id = f.Id, Name = f.Name, ContentType = f.ContentType, Size = f.Size,
			})
			.Take(count)
			.ToArrayAsync(ct);

		return TypedResults.Ok(files.AsEnumerable());
	}
}
