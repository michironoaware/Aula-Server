using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Shared.Results;

internal sealed class Result<TValue>
{
	private readonly TValue? _value;

	internal Result(TValue value)
	{
		_value = value;
		Succeeded = true;
	}

	internal Result(ResultError resultError)
	{
		ResultError = resultError;
		Succeeded = false;
	}

	[MemberNotNullWhen(false, nameof(ResultError))]
	internal Boolean Succeeded { get; }

	internal ResultError? ResultError { get; }

	internal TValue Value => Succeeded ? _value! : throw new InvalidOperationException("Result did not succeed.");

	public static implicit operator Result<TValue>(TValue value) => new(value);

	public static implicit operator Result<TValue>(ResultError resultError) => new(resultError);

	public static explicit operator TValue(Result<TValue> result) => result.Value;
}
