namespace Aula.Server.Shared.Results;

internal sealed class ValidationResultError : ResultError
{
	internal ValidationResultError(String field, String description)
	{
		Field = field;
		Description = description;
	}

	internal String Field { get; }

	internal String Description { get; }
}
