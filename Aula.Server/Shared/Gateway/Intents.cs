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
	///                 <see cref="EventType.RoomRemoved" />
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
	///                 <see cref="EventType.MessageRemoved" />
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
	///                 <see cref="EventType.BanRemoved" />
	///             </term>
	///         </item>
	///     </list>
	/// </summary>
	Moderation = 1 << 3,
}
