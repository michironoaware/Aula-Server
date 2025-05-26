using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Shared.Types;

/// <summary>Encapsulates a value that can be present or not. Similar to <see cref="Nullable{T}" />.</summary>
/// <typeparam name="T">The underlying type of the value.</typeparam>
internal readonly struct Option<T> : IEquatable<Option<T>>
{
	private readonly T? _value;

	/// <summary>Initializes a new instance of the <see cref="Option{T}" /> in an unassigned state.</summary>
	public Option()
	{ }

	/// <summary>Initializes a new instance of the <see cref="Option{T}" /> struct with the specified value.</summary>
	/// <param name="value">The value to be assigned to this instance.</param>
	internal Option(T value)
	{
		HasValue = true;
		_value = value;
	}

	/// <summary>
	///     Gets the underlying value of the current <see cref="Option{T}" /> instance.
	///     If not present an <see cref="InvalidOperationException" /> will be thrown.
	/// </summary>
	internal T Value => HasValue ? _value! : throw new InvalidOperationException("Value is not assigned.");

	/// <summary>Gets a value indicating whether the current <see cref="Option{T}" /> instance has a value.</summary>
	internal Boolean HasValue { get; }

	/// <summary>Encapsulates a value into a new instance of <see cref="Option{T}" />.</summary>
	/// <param name="value">The value to encapsulate.</param>
	/// <returns>a new <see cref="Option{T}" /> instance containing the underlying value.</returns>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates",
		Justification = "Use constructor as named alternate.")]
	public static implicit operator Option<T>(T value) => new(value);

	/// <summary>Gets underlying value of a <see cref="Option{T}" /> instance.</summary>
	/// <param name="value">The <see cref="Option{T}" /> instance.</param>
	/// <returns>The underlying value of the <see cref="Option{T}" /> instance.</returns>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates",
		Justification = "Use constructor as named alternate.")]
	public static explicit operator T(Option<T> value) => value.Value;

	/// <summary>Compares two <see cref="Option{T}" /> for equality.</summary>
	/// <param name="left">The left-hand value.</param>
	/// <param name="right">The right-hand value.</param>
	/// <returns>true if <paramref name="left" /> is equal to <paramref name="right" />; otherwise false.</returns>
	public static Boolean operator ==(Option<T> left, Option<T> right) => left.Equals(right);

	/// <summary>Compares two <see cref="Option{T}" /> for equality.</summary>
	/// <param name="left">The left-hand value.</param>
	/// <param name="right">The right-hand value.</param>
	/// <returns>false if <paramref name="left" /> is equal to <paramref name="right" />; otherwise true.</returns>
	public static Boolean operator !=(Option<T> left, Option<T> right) => !(left == right);

	/// <inheritdoc />
	public override Boolean Equals(Object? obj) => obj is Option<T> other && Equals(other);

	/// <inheritdoc />
	public Boolean Equals(Option<T> other)
	{
		if (HasValue != other.HasValue)
			return false;

		if (Value is not null)
			return Value.Equals(other.Value);

		return other.Value is null;
	}

	/// <inheritdoc />
	public override Int32 GetHashCode() => HashCode.Combine(_value, HasValue);
}
