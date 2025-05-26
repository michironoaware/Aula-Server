namespace Aula.Server.Shared.Results;

internal sealed class NotFoundResultError : ResultError
{
	private NotFoundResultError()
	{ }

	internal static NotFoundResultError Instance { get; } = new();
}
