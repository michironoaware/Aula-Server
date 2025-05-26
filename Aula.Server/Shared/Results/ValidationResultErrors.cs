namespace Aula.Server.Shared.Results;

internal sealed class ValidationResultErrors : ResultError
{
	internal ValidationResultErrors(String title, IEnumerable<ValidationResultError> errors)
	{
		Title = title;
		Errors = errors.ToList().AsReadOnly();
	}

	internal String Title { get; }

	internal IReadOnlyList<ValidationResultError> Errors { get; }
}
