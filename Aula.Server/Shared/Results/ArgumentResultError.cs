namespace Aula.Server.Shared.Results;

internal sealed class ArgumentResultError : ResultError
{
	internal ArgumentResultError(String title, String details)
	{
		Title = title;
		Details = details;
	}

	internal String Title { get; }

	internal String Details { get; }
}
