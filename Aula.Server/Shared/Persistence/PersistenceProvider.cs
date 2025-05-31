using System.Text.Json.Serialization;

namespace Aula.Server.Shared.Persistence;

[JsonConverter(typeof(JsonStringEnumConverter<PersistenceProvider>))]
internal enum PersistenceProvider
{
	Sqlite,
	Postgres,
}
