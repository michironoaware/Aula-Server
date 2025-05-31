namespace Aula.Server.Shared.Persistence;

/// <summary>
///     Persistence and database related configurations.
/// </summary>
internal sealed class PersistenceOptions
{
	internal const String SectionName = "Persistence";

	/// <summary>
	///     The connection string used to connect to the database.
	/// </summary>
	public String ConnectionString { get; set; } = "DataSource=./Persistence.db";

	/// <summary>
	///     The persistence provider to use.
	/// </summary>
	public PersistenceProvider Provider { get; set; } = PersistenceProvider.Sqlite;

	public Int32 PostgresVersionMajor { get; set; } = 16;

	public Int32 PostgresVersionMinor { get; set; }
}
