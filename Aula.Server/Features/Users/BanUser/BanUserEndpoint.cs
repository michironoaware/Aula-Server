using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Bans;
using Aula.Server.Features.Users.Shared;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Users.BanUser;

internal sealed class BanUserEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("users/{userId}/bans", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<BanData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromBody] BanUserRequestBody body,
		[FromServices] IValidator<BanUserRequestBody> bodyValidator,
		[FromServices] UserManager userManager,
		[FromServices] AppDbContext dbContext,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] IPublisher publisher,
		HttpContext httpContext,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var validation = await bodyValidator.ValidateAsync(body, ct);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var currentUserId = userManager.GetUserId(httpContext.User);
		if (currentUserId is null)
			return TypedResults.InternalServerError();
		if (currentUserId == userId)
			return TypedResults.Problem(ProblemDetailsDefaults.TargetIsSelf);

		var alreadyBanned = await dbContext.Bans
			.OfType<UserBan>()
			.Where(b => b.TargetUserId == userId && !b.IsLifted)
			.AnyAsync(ct);
		if (alreadyBanned)
			return TypedResults.Problem(ProblemDetailsDefaults.UserAlreadyBanned);

		var targetUser = await dbContext.Users
			.Where(u => u.Id == userId)
			.Select(u => new { Permissions = u.RoleAssignments.Select(ra => ra.Role.Permissions) })
			.FirstOrDefaultAsync(ct);
		if (targetUser is null)
			return TypedResults.Problem(ProblemDetailsDefaults.TargetDoesNotExist);
		if (await userManager.HasPermissionAsync(targetUser.Permissions, Permissions.Administrator))
			return TypedResults.Problem(ProblemDetailsDefaults.TargetIsAdministrator);

		var ban = new UserBan(await snowflakes.GenerateAsync(), currentUserId, body.Reason, userId!);

		_ = dbContext.Bans.Add(ban);
		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new BanIssuedEvent(ban), CancellationToken.None);

		return TypedResults.Ok(ban.ToBanData());
	}
}
