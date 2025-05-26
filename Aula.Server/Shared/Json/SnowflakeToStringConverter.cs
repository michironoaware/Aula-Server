using System.Text.Json;
using System.Text.Json.Serialization;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Shared.Json;

internal sealed class SnowflakeToStringConverter : JsonConverter<Snowflake>
{
	public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.Number => reader.GetUInt64(),
			JsonTokenType.String => UInt64.Parse(reader.ValueSpan),
			_ => throw new JsonException(),
		};
	}

	public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
