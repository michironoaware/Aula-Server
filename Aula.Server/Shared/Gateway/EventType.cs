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
	///     A room has been deleted.
	/// </summary>
	RoomDeleted,

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
	MessageDeleted,

	/// <summary>
	///     A user has started typing in a room.
	/// </summary>
	UserStartedTyping,

	/// <summary>
	///     A ban has been issued.
	/// </summary>
	BanIssued,

	/// <summary>
	///     A ban has been lifted.
	/// </summary>
	BanLifted,

	/// <summary>
	///     A role has been created.
	/// </summary>
	RoleCreated,

	/// <summary>
	///     A role has been updated.
	/// </summary>
	RoleUpdated,

	/// <summary>
	///     A role has been deleted.
	/// </summary>
	RoleDeleted,

	#endregion

	#region Receive

	/// <summary>
	///     Updates the current presence status for the current user.
	/// </summary>
	UpdatePresence,

	#endregion
}
