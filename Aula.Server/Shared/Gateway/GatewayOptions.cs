using System.ComponentModel.DataAnnotations;

namespace Aula.Server.Shared.Gateway;

internal sealed class GatewayOptions
{
	internal const String SectionName = "Gateway";

	[Range(1, Int32.MaxValue)]
	public Int32 SecondsToExpire { get; set; } = 60 * 5;
}
