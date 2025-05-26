using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Shared.Results;

internal sealed class Result
{
	private static readonly Result s_success = new();

	internal Result(ResultError resultError)
	{
		ResultError = resultError;
		Succeeded = false;
	}

	private Result()
	{
		Succeeded = true;
	}

	[MemberNotNullWhen(false, nameof(ResultError))]
	internal Boolean Succeeded { get; }

	internal ResultError? ResultError { get; }

	internal static Result Success() => s_success;

	internal static Result<TValue> Success<TValue>(TValue value) => new(value);

	public static implicit operator Result(ResultError resultError) => new(resultError);
}
