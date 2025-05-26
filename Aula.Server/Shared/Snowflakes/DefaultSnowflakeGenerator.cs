using Microsoft.Extensions.Options;

namespace Aula.Server.Shared.Snowflakes;

internal sealed class DefaultSnowflakeGenerator : ISnowflakeGenerator
{
	private static readonly DateTime s_epoch = new(2024, 12, 1, 12, 0, 0, DateTimeKind.Utc);
	private static readonly TimeSpan s_oneTickSpan = TimeSpan.FromTicks(1);
	private readonly Lock _newSnowflakeLock = new();
	private readonly UInt16 _workerId;
	private UInt16 _increment;
	private DateTime _lastOperationDate;

	public DefaultSnowflakeGenerator(IOptions<SnowflakeOptions> applicationOptions)
	{
		var workerId = applicationOptions.Value.WorkerId;
		_workerId = workerId;
		_lastOperationDate = DateTime.UtcNow;
	}

	public Snowflake Generate()
	{
		_newSnowflakeLock.Enter();

		if (++_increment > Snowflake.MaxIncrement)
		{
			while (_lastOperationDate == DateTime.UtcNow)
			{ }

			_increment = 0;
		}

		_lastOperationDate = DateTime.UtcNow;
		var snowflake = new Snowflake(s_epoch, _lastOperationDate, _workerId, _increment++);

		_newSnowflakeLock.Exit();
		return snowflake;
	}

	public async ValueTask<Snowflake> GenerateAsync()
	{
		_newSnowflakeLock.Enter();

		if (++_increment > Snowflake.MaxIncrement)
		{
			while (_lastOperationDate == DateTime.UtcNow)
				await Task.Delay(s_oneTickSpan);

			_increment = 0;
		}

		_lastOperationDate = DateTime.UtcNow;
		var snowflake = new Snowflake(s_epoch, _lastOperationDate, _workerId, _increment++);

		_newSnowflakeLock.Exit();
		return snowflake;
	}
}
