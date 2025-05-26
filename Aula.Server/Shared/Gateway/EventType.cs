using System.Text.Json.Serialization;

namespace Aula.Server.Shared.Gateway;

/// <summary>
///     The name of the events that can be dispatched in a gateway session.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EventType>))]
internal enum EventType
{
	#region Send

	/// <summary>
	///     The gateway connection is ready.
	/// </summary>
	Ready,

	/// <summary>
	///     A new room has been created.
	/// </summary>
	RoomCreated,

	/// <summary>
	///     A room has been updated.
	/// </summary>
	RoomUpdated,

	/// <summary>
	///     A room has been removed.
	/// </summary>
	RoomRemoved,

	/// <summary>
	///     A user has been updated.
	/// </summary>
	UserUpdated,

	/// <summary>
	///     A user has moved from room.
	/// </summary>
	UserCurrentRoomUpdated,

	/// <summary>
	///     A user presence has been updated.
	/// </summary>
	UserPresenceUpdated,

	/// <summary>
	///     A new message has been sent.
	/// </summary>
	MessageCreated,

	/// <summary>
	///     A message has been deleted.
	/// </summary>
	MessageRemoved,

	/// <summary>
	///     A user has started typing in a room.
	/// </summary>
	UserStartedTyping,

	/// <summary>
	///     A user has been banned.
	/// </summary>
	BanIssued,

	/// <summary>
	///     A user has been unbanned.
	/// </summary>
	BanRemoved,

	#endregion

	#region Receive

	/// <summary>
	///     Updates the current presence status for the current user.
	/// </summary>
	UpdatePresence,

	#endregion
}
