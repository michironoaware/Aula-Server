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

namespace Aula.Server.Features.Rooms.GetRoomResidents;

internal sealed class GetRoomResidentsEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("/rooms/{roomId}/residents", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<UserData>>, NotFound>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromServices] AppDbContext dbContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var roomExists = await dbContext.Rooms
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.AnyAsync(ct);
		if (!roomExists)
			return TypedResults.NotFound();

		var users = await dbContext.Users
			.Where(u => u.CurrentRoomId == roomId)
			.Select(u => new UserData
			{
				Id = u.Id,
				DisplayName = u.DisplayName,
				Description = u.Description,
				Type = u.Type,
				Presence = u.Presence,
				RoleIds = u.RoleAssignments.Select(r => r.RoleId),
				CurrentRoomId = u.CurrentRoomId,
			})
			.ToArrayAsync(ct);

		return TypedResults.Ok(users.AsEnumerable());
	}
}
