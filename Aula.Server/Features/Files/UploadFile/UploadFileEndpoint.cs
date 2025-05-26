using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Files;
using Aula.Server.Shared.ApiContracts;
using Aula.Server.Shared.Authorization;
using Aula.Server.Shared.Endpoints;
using Aula.Server.Shared.Extensions;
using Aula.Server.Shared.Identity;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Features.Files.UploadFile;

internal sealed class UploadFileEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("files", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireConfirmedEmail()
			.RequirePermissions(Permissions.UploadFiles)
			.RequireRateLimiting(FileRateLimitingPolicy.Name)
			.DenyBannedUsers()
			.DisableAntiforgery()
			.HasApiVersion(1);
	}

	private static async Task<Results<Created<FileData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromForm] IFormFile file,
		[FromServices] FileNameValidator nameValidator,
		[FromServices] FileValidator fileValidator,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] IPublisher publisher,
		[FromServices] AppDbContext dbContext,
		[FromServices] UserManager userManager,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var nameValidation = await nameValidator.ValidateAsync(file.FileName, ct);
		var fileValidation = await fileValidator.ValidateAsync(file, ct);
		if (!nameValidation.IsValid ||
		    !fileValidation.IsValid)
		{
			var errors = nameValidation.Errors.Concat(fileValidation.Errors);
			var problemDetails = errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var userId = userManager.GetUserId(httpContext.User);
		if (userId is null)
			return TypedResults.InternalServerError();

		var fileContent = await file.OpenReadStream().ToArrayAsync(ct);
		var domainFile = new File(await snowflakes.GenerateAsync(), file.FileName, file.ContentType, fileContent,
			(Snowflake)userId);

		_ = dbContext.Add(domainFile);
		_ = await dbContext.SaveChangesAsync(ct);
		await publisher.Publish(new FileCreatedEvent(domainFile), CancellationToken.None);

		return TypedResults.Created($"{httpContext.Request.GetUrl()}/{domainFile.Id}", new FileData
		{
			Id = domainFile.Id, Name = domainFile.Name, ContentType = domainFile.ContentType, Size = domainFile.Size,
		});
	}
}
