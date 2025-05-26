namespace Aula.Server.Shared.Resilience;

internal static class ResiliencePipelines
{
	internal const String RetryOnDbConcurrencyProblem = $"{Prefix}.{nameof(RetryOnDbConcurrencyProblem)}";
	private const String Prefix = nameof(ResiliencePipelines);
}
