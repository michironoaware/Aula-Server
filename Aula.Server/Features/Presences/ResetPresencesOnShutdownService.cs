using Aula.Server.Domain.Users;
using Aula.Server.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Presences;

internal sealed class ResetPresencesOnShutdownService : IHostedService, IDisposable
{
	private readonly IServiceScope _serviceScope;

	public ResetPresencesOnShutdownService(IServiceProvider serviceProvider)
	{
		_serviceScope = serviceProvider.CreateScope();
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await ResetPresencesAsync(cancellationToken);
	}

	~ResetPresencesOnShutdownService()
	{
		Dispose(false);
	}

	private async Task ResetPresencesAsync(CancellationToken cancellationToken)
	{
		var dbContext = _serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
		_ = await dbContext.Users
			.ExecuteUpdateAsync(setPropertyCalls => setPropertyCalls
				.SetProperty(property => property.Presence, value => Presence.Offline), cancellationToken);
	}

	private void Dispose(Boolean disposing)
	{
		if (disposing)
			_serviceScope.Dispose();
	}
}
