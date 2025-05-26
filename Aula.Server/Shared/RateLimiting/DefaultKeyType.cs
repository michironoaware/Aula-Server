namespace Aula.Server.Shared.RateLimiting;

internal readonly struct DefaultKeyType : IEquatable<DefaultKeyType>
{
	internal DefaultKeyType(String? policyName, Object? key, Object? factory = null)
	{
		PolicyName = policyName;
		Key = key;
		Factory = factory;
	}

	internal String? PolicyName { get; }

	internal Object? Key { get; }

	// This is really a Func<TPartitionKey, RateLimiter>
	internal Object? Factory { get; }

	public static Boolean operator ==(DefaultKeyType left, DefaultKeyType right) => left.Equals(right);

	public static Boolean operator !=(DefaultKeyType left, DefaultKeyType right) => !left.Equals(right);

	public Boolean Equals(DefaultKeyType other) => PolicyName == other.PolicyName && Equals(Key, other.Key);

	public override Boolean Equals(Object? obj) => obj is DefaultKeyType other && Equals(other);

	public override Int32 GetHashCode() => HashCode.Combine(PolicyName, Key);
}
