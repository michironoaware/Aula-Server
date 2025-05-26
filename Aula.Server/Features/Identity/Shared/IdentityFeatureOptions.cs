namespace Aula.Server.Features.Identity.Shared;

/// <summary>
///     User identity related configurations.
/// </summary>
internal sealed class IdentityFeatureOptions
{
	internal const String SectionName = "Identity";

	/// <summary>
	///     The Uri to redirect to after users confirm their emails, should be a trusted origin.
	/// </summary>
	public Uri? ConfirmEmailRedirectUri { get; set; }

	/// <summary>
	///     The Uri where users should reset their password, should be a trusted origin.
	/// </summary>
	public Uri? ResetPasswordRedirectUri { get; set; }
}
