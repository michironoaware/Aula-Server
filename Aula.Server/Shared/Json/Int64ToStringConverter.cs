using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aula.Server.Shared.Json;

internal sealed class Int64ToStringConverter : JsonConverter<Int64>
{
	public override Int64 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType is not JsonTokenType.String ||
		    !Int64.TryParse(reader.ValueSpan, out var result))
			throw new JsonException();

		return result;
	}

	public override void Write(Utf8JsonWriter writer, Int64 value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
