using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Features.Users.ModifyUser;

/// <summary>
///     Holds the data required to update a user.
/// </summary>
internal sealed record ModifyUserRequestBody
{
	/// <summary>
	///     The new name for the user.
	/// </summary>
	public String? DisplayName { get; init; }

	/// <summary>
	///     The new description for the user.
	/// </summary>
	public String? Description { get; init; }

	/// <summary>
	///     The id of the room where the user will be relocated.
	/// </summary>
	public Snowflake? CurrentRoomId { get; init; }

	/// <summary>
	///     The id of the roles for the user.
	/// </summary>
	public IReadOnlyList<Snowflake>? RoleIds { get; init; }
}
