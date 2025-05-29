using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Aula.Server.Features.Files.GetFileContent;

internal sealed class FileContentCache
{
	private static readonly TimeSpan s_cacheEntrySlidingExpiration = TimeSpan.FromMinutes(15);
	private readonly IMemoryCache _cache;
	private readonly AppDbContext _dbContext;

	public FileContentCache(IMemoryCache cache, AppDbContext dbContext)
	{
		_cache = cache;
		_dbContext = dbContext;
	}

	internal async ValueTask<Byte[]?> GetByIdAsync(Snowflake fileId, CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		var cacheKey = $"FileContent:{fileId}";
		if (!_cache.TryGetValue(cacheKey, out var cachedFileContent))
		{
			var fileContent = await _dbContext.Files
				.Where(f => f.Id == fileId)
				.Select(f => f.Content)
				.FirstOrDefaultAsync(ct);
			if (fileContent is null)
				return null;

			using var cacheEntry = _cache.CreateEntry(cacheKey);
			cacheEntry.SlidingExpiration = s_cacheEntrySlidingExpiration;
			cacheEntry.Value = fileContent;
			return fileContent;
		}

		return (Byte[])cachedFileContent!;
	}
}
