using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aula.Server.Shared.Json;

internal sealed class UInt64ToStringConverter : JsonConverter<UInt64>
{
	public override UInt64 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType is not JsonTokenType.String ||
		    !UInt64.TryParse(reader.ValueSpan, out var result))
			throw new JsonException();

		return result;
	}

	public override void Write(Utf8JsonWriter writer, UInt64 value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
