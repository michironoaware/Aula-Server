using System.Text.Json;

namespace Aula.Server.Shared.Json;

internal static class ObjectExtensions
{
	internal static Byte[] GetJsonUtf8Bytes<TValue>(this TValue value, JsonSerializerOptions options) =>
		JsonSerializer.SerializeToUtf8Bytes(value, options);
}
