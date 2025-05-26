using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Shared.Snowflakes;

internal readonly struct Snowflake : ISpanParsable<Snowflake>, IEquatable<Snowflake>
{
	internal const UInt64 MaxMillisecondsSinceEpoch = 0b11_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;
	internal const UInt16 MaxWorkerId = 0b11_1111_1111;
	internal const UInt16 MaxIncrement = 0b1111_1111_1111;

	internal Snowflake(UInt64 value)
	{
		ArgumentOutOfRangeException.ThrowIfZero(value);
		Value = value;
	}

	internal Snowflake(UInt64 millisecondsSinceEpoch, UInt16 workerId, UInt16 increment)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(millisecondsSinceEpoch, MaxMillisecondsSinceEpoch,
			nameof(millisecondsSinceEpoch));
		ArgumentOutOfRangeException.ThrowIfGreaterThan(workerId, MaxWorkerId, nameof(workerId));
		ArgumentOutOfRangeException.ThrowIfGreaterThan(increment, MaxIncrement, nameof(increment));

		Value = (millisecondsSinceEpoch << 22) | ((UInt32)workerId << 12) | increment;
	}

	internal Snowflake(DateTime epoch, DateTime instant, UInt16 workerId, UInt16 increment)
		: this((UInt64)(instant - epoch).TotalMilliseconds, workerId, increment)
	{ }

	internal UInt64 Value { get; }

	internal UInt16 Increment => (UInt16)(Value & 0b1111_1111_1111);

	internal UInt16 WorkerId => (UInt16)((Value >> 12) & 0b11_1111_1111);

	internal UInt64 MillisecondsSinceEpoch => Value >> 22;

	public static implicit operator Snowflake(UInt64 value) => new(value);

	public static implicit operator UInt64(Snowflake value) => value.Value;

	public static Boolean operator ==(Snowflake left, Snowflake right) => left.Equals(right);

	public static Boolean operator !=(Snowflake left, Snowflake right) => !left.Equals(right);

	public static Snowflake Parse(String s, IFormatProvider? provider) => UInt64.Parse(s, provider);

	public static Boolean TryParse([NotNullWhen(true)] String? s, IFormatProvider? provider, out Snowflake value)
	{
		var success = UInt64.TryParse(s, provider, out var number);
		value = new Snowflake(number);
		return success;
	}

	public static Snowflake Parse(ReadOnlySpan<Char> s, IFormatProvider? provider) => UInt64.Parse(s, provider);

	public static Boolean TryParse(ReadOnlySpan<Char> s, IFormatProvider? provider, out Snowflake value)
	{
		var success = UInt64.TryParse(s, provider, out var number);
		if (number is 0)
		{
			value = default;
			return false;
		}

		value = new Snowflake(number);
		return success;
	}

	public static Boolean TryParse(ReadOnlySpan<Char> s, out Snowflake value)
	{
		var success = UInt64.TryParse(s, out var number);
		if (number is 0)
		{
			value = default;
			return false;
		}

		value = new Snowflake(number);
		return success;
	}

	public Boolean Equals(Snowflake other) => Value == other.Value;

	public override Boolean Equals(Object? obj) => obj is Snowflake other && Equals(other);

	public override Int32 GetHashCode() => Value.GetHashCode();

	public override String ToString() => Value.ToString();
}
