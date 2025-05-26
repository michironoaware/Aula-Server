using System.Threading.RateLimiting;

namespace Aula.Server.Shared.RateLimiting;

internal sealed class ExtendedReplenishingRateLimiter : ReplenishingRateLimiter
{
	private readonly ReplenishingRateLimiter _underlyingRateLimiter;

	internal ExtendedReplenishingRateLimiter(ReplenishingRateLimiter underlyingRateLimiter)
	{
		_underlyingRateLimiter = underlyingRateLimiter;
	}

	public override TimeSpan? IdleDuration => _underlyingRateLimiter.IdleDuration;

	public override Boolean IsAutoReplenishing => _underlyingRateLimiter.IsAutoReplenishing;

	public override TimeSpan ReplenishmentPeriod => _underlyingRateLimiter.ReplenishmentPeriod;

	internal DateTime? FirstWindowAcquireDate { get; private set; }

	internal DateTime? ReplenishmentDate => FirstWindowAcquireDate + _underlyingRateLimiter.ReplenishmentPeriod;

	public override RateLimiterStatistics? GetStatistics() => _underlyingRateLimiter.GetStatistics();

	public override Boolean TryReplenish()
	{
		var replenished = _underlyingRateLimiter.TryReplenish();
		if (replenished)
			FirstWindowAcquireDate = null;

		return replenished;
	}

	protected override async ValueTask<RateLimitLease> AcquireAsyncCore(
		Int32 permitCount,
		CancellationToken cancellationToken)
	{
		ReplenishIfAcceptable();
		FirstWindowAcquireDate ??= DateTime.UtcNow;
		return await _underlyingRateLimiter.AcquireAsync(permitCount, cancellationToken);
	}

	protected override RateLimitLease AttemptAcquireCore(Int32 permitCount)
	{
		ReplenishIfAcceptable();
		FirstWindowAcquireDate ??= DateTime.UtcNow;
		return _underlyingRateLimiter.AttemptAcquire(permitCount);
	}

	private void ReplenishIfAcceptable()
	{
		var now = DateTime.UtcNow;
		if (ReplenishmentDate is not null &&
		    FirstWindowAcquireDate is not null &&
		    now - FirstWindowAcquireDate > ReplenishmentPeriod)
			_ = TryReplenish();
	}
}
