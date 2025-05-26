using Aula.Server.Domain.Users;

namespace Aula.Server.Shared.Identity;

/// <summary>
///     Represents the outcome of a register operation.
/// </summary>
internal sealed class RegisterUserResult
{
	private RegisterUserResult(String description, Boolean succeeded)
	{
		Description = description;
		Succeeded = succeeded;
	}

	/// <summary>
	///     The operation succeeded.
	/// </summary>
	internal static RegisterUserResult Success { get; } = new("Register succeeded", true);

	/// <summary>
	///     The email provided is already in use.
	/// </summary>
	internal static RegisterUserResult EmailInUse { get; } = new("The email provided is already in use.", false);

	/// <summary>
	///     The username provided is already in use.
	/// </summary>
	internal static RegisterUserResult UserNameInUse { get; } = new("The username provided is already in use.", false);

	/// <summary>
	///     The username provided contains invalid characters.
	/// </summary>
	internal static RegisterUserResult InvalidUserNameChar { get; } =
		new($"Usernames can only contain the following characters: {StandardUser.UserNameValidChars}", false);

	/// <summary>
	///     A more detailed description of the result.
	/// </summary>
	internal String Description { get; }

	/// <summary>
	///     Whether the operation succeeded.
	/// </summary>
	internal Boolean Succeeded { get; private set; }
}
