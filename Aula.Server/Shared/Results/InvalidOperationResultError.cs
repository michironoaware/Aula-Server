namespace Aula.Server.Shared.Results;

internal sealed class InvalidOperationResultError : ResultError
{
	internal InvalidOperationResultError(String message)
	{
		Message = message;
	}

	internal String Message { get; }
}
