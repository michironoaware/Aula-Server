namespace Aula.Server.Shared.Gateway;

/// <summary>
///     Gateway intents define which events will be dispatched during the session.
/// </summary>
[Flags]
internal enum Intents : UInt64
{
	/// <summary>
	///     <list type="bullet">
	///         <item>
	///             <term>
	///                 <see cref="EventType.UserUpdated" />
	///             </term>
	///         </item>
	///         <item>
	///             <term>
	///                 <see cref="EventType.UserCurrentRoomUpdated" />
	///             </term>
	///         </item>
	///     </list>
	/// </summary>
	Users = 1 << 0,

	/// <summary>
	///     <list type="bullet">
	///         <item>
	///             <term>
	///                 <see cref="EventType.RoomCreated" />
	///             </term>
	///         </item>
	///         <item>
	///             <term>
	///                 <see cref="EventType.RoomUpdated" />
	///             </term>
	///         </item>
	///         <item>
	///             <term>
	///                 <see cref="EventType.RoomDeleted" />
	///             </term>
	///         </item>
	///         <item>
	///             <term>
	///                 <see cref="EventType.RoomConnectionCreated" />
	///             </term>
	///         </item>
	///         <item>
	///             <term>
	///                 <see cref="EventType.RoomConnectionRemoved" />
	///             </term>
	///         </item>
	///     </list>
	/// </summary>
	Rooms = 1 << 1,

	/// <summary>
	///     <list type="bullet">
	///         <item>
	///             <term>
	///                 <see cref="EventType.MessageCreated" />
	///             </term>
	///         </item>
	///         <item>
	///             <term>
	///                 <see cref="EventType.MessageDeleted" />
	///             </term>
	///         </item>
	///     </list>
	/// </summary>
	Messages = 1 << 2,

	/// <summary>
	///     <list type="bullet">
	///         <item>
	///             <term>
	///                 <see cref="EventType.BanIssued" />
	///             </term>
	///         </item>
	///         <item>
	///             <term>
	///                 <see cref="EventType.BanLifted" />
	///             </term>
	///         </item>
	///     </list>
	/// </summary>
	Moderation = 1 << 3,
}
