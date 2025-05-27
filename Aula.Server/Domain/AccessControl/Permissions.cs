namespace Aula.Server.Domain.AccessControl;

/// <summary>
///     Enumerates permissions that can be assigned to a user within the application.
/// </summary>
[Flags]
internal enum Permissions : UInt64
{
	None = 0,

	/// <summary>
	///     Grants the user privileges over the entire application.
	/// </summary>
	Administrator = 1 << 0,

	/// <summary>
	///     Allows to create, modify and remove rooms.
	/// </summary>
	ManageRooms = 1 << 1,

	/// <summary>
	///     Allows retrieving previously sent messages.
	/// </summary>
	ReadMessages = 1 << 2,

	/// <summary>
	///     Allows to send messages.
	/// </summary>
	SendMessages = 1 << 3,

	/// <summary>
	///     Allows to delete messages sent by other users.
	/// </summary>
	ManageMessages = 1 << 4,

	/// <summary>
	///     Allows to set the current room of any user.
	/// </summary>
	SetCurrentRoom = 1 << 5,

	/// <summary>
	///     Allows to ban and unban users from the application.
	/// </summary>
	BanUsers = 1 << 6,

	/// <summary>
	///     Allows to upload files to the application.
	/// </summary>
	UploadFiles = 1 << 7,

	/// <summary>
	///     Allows to modify and remove files.
	/// </summary>
	ManageFiles = 1 << 8,

	/// <summary>
	///     Allows to create, modify, remove and assign roles.
	/// </summary>
	ManageRoles = 1 << 9,

	/// <summary>
	///     Allows selecting a custom presence when connecting to the gateway.
	/// </summary>
	UpdatePresence = 1 << 10,
}
