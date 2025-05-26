namespace Aula.Server.Shared.Snowflakes;

internal interface ISnowflakeGenerator
{
	Snowflake Generate();

	ValueTask<Snowflake> GenerateAsync();
}
