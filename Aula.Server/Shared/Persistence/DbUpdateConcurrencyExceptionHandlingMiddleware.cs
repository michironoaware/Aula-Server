using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Shared.Persistence;

internal sealed class DbUpdateConcurrencyExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;

	public DbUpdateConcurrencyExceptionHandlingMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext httpContext)
	{
		try
		{
			await _next(httpContext);
		}
		catch (DbUpdateConcurrencyException)
		{
			httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
		}
	}
}
