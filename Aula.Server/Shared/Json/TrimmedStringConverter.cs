using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aula.Server.Shared.Json;

internal sealed class TrimmedStringConverter : JsonConverter<String?>
{
	public override String? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
			throw new JsonException("Expected a string or null.");

		return reader.GetString()?.Trim();
	}

	public override void Write(Utf8JsonWriter writer, String? value, JsonSerializerOptions options)
	{
		if (value is null)
			writer.WriteNullValue();

		writer.WriteStringValue(value);
	}
}
