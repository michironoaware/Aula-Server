namespace Aula.Server.Shared.Results;

internal sealed class AggregateResultError : ResultError
{
	internal AggregateResultError(IEnumerable<ResultError> errors)
	{
		Errors = errors.ToList().AsReadOnly();
	}

	internal IReadOnlyList<ResultError> Errors { get; }
}
