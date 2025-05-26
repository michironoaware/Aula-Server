using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Bans;
using Aula.Server.Features.Bans.Shared;
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

namespace Aula.Server.Features.Bans.GetBans;

internal sealed class GetBansEndpoint : IApiEndpoint
{
	internal const String TypeQueryParamName = "type";
	internal const String AfterQueryParamName = "after";
	internal const String CountQueryParamName = "count";
	internal const String IncludeLiftedQueryParamName = "includeLifted";
	internal const Int32 MinimumBanCount = 1;
	internal const Int32 MaximumBanCount = 100;
	internal const Int32 DefaultBanCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<BanData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = TypeQueryParamName)] BanType? banType,
		[FromQuery(Name = AfterQueryParamName)] Snowflake? afterBanId,
		[FromQuery(Name = CountQueryParamName)] Int32? specifiedCount,
		[FromQuery(Name = IncludeLiftedQueryParamName)] Boolean? includeLifted,
		[FromServices] AppDbContext dbContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var count = specifiedCount ?? DefaultBanCount;
		if (specifiedCount is > MaximumBanCount or < MinimumBanCount)
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidBanCount);

		var bansQuery = dbContext.Bans
			.OrderBy(r => r.EmissionDate)
			.AsQueryable();

		if (banType is not null)
			bansQuery = bansQuery.Where(r => r.Type == banType);

		if (!includeLifted ?? false)
			bansQuery = bansQuery.Where(b => !b.IsLifted);

		if (afterBanId is not null)
		{
			var afterBan = await dbContext.Bans
				.Where(b => b.Id == afterBanId)
				.Select(b => new { b.EmissionDate })
				.FirstOrDefaultAsync(ct);
			if (afterBan is null)
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterBan);

			bansQuery = bansQuery.Where(b => b.EmissionDate > afterBan.EmissionDate);
		}

		var bans = await bansQuery
			.AsNoTracking()
			.Take(count)
			.ToArrayAsync(ct);
		return TypedResults.Ok(bans.Select(b => b.ToBanData()));
	}
}
