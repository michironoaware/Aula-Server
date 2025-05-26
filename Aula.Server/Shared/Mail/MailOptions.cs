using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Shared.Mail;

/// <summary>
///     Mail related configurations.
/// </summary>
internal sealed class MailOptions
{
	internal const String SectionName = "Mail";

	/// <summary>
	///     The address to use for sending emails.
	/// </summary>
	[Required]
	public required String Address { get; set; }

	/// <summary>
	///     The password of the address.
	/// </summary>
	[Required]
	public required String Password { get; set; }

	/// <summary>
	///     The SMTP host.
	/// </summary>
	[Required]
	public required String SmtpHost { get; set; }

	/// <summary>
	///     The SMTP host port.
	/// </summary>
	[Required]
	[NotNull]
	public required Int32? SmtpPort { get; set; }

	/// <summary>
	///     If SSL should be enabled when sending emails.
	/// </summary>
	[Required]
	[NotNull]
	public required Boolean? EnableSsl { get; set; }
}
