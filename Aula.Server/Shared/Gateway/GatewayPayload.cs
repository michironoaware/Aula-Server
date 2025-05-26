namespace Aula.Server.Shared.Gateway;

/// <summary>
///     Represents a payload sent over a Gateway connection.
/// </summary>
/// <typeparam name="TData">The type of the Data</typeparam>
internal class GatewayPayload<TData>
{
	/// <summary>
	///     The gateway operation code, which indicates the payload type.
	/// </summary>
	public required OperationType Operation { get; init; }

	/// <summary>
	///     The name of the event dispatched. Only present for <see cref="OperationType.Dispatch" /> payloads.
	/// </summary>
	public EventType? Event { get; init; }

	/// <summary>
	///     The data of the payload.
	/// </summary>
	public TData? Data { get; init; }
}
