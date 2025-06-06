using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Aula.Server.Shared.Snowflakes;

namespace Aula.Server.Domain.Users;

internal class StandardUser : User
{
	internal const String UserNameValidChars = $"{LowercaseCharacters}{Digits}._";
	internal const Int32 UserNameMinLength = 6;
	internal const Int32 UserNameMaxLength = 32;
	internal const Int32 EmailMaxLength = 1024;
	internal const Int32 PasswordMaxLength = 128;
	private const String LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
	private const String Digits = "0123456789";

	[SetsRequiredMembers]
	internal StandardUser(
		Snowflake id,
		String displayName,
		String description,
		String userName,
		String email)
		: base(id, UserType.Standard, displayName, description)
	{
		UserName = userName;
		Email = email;
	}

	// EntityFramework constructor
	private StandardUser()
	{ }

	[Length(UserNameMinLength, UserNameMaxLength)]
	public required String UserName { get; set; }

	[MaxLength(EmailMaxLength)]
	public required String Email { get; set; }

	public required Boolean EmailConfirmed { get; set; }

	public String? PasswordHash { get; set; }

	public required Int32 AccessFailedCount { get; set; }

	public DateTime? LockoutEndTime { get; set; }
}
