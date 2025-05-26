namespace Aula.Server.Shared.RateLimiting;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class IgnoreRateLimitingAttribute : Attribute
{ }
