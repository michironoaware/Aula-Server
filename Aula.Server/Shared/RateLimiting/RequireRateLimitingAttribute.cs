namespace Aula.Server.Shared.RateLimiting;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RequireRateLimitingAttribute : Attribute
{
	internal RequireRateLimitingAttribute(String policyName)
	{
		PolicyName = policyName;
	}

	internal String PolicyName { get; }
}
