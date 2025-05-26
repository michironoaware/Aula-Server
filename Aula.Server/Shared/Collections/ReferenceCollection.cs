using System.Runtime.CompilerServices;

namespace Aula.Server.Shared.Collections;

internal struct ReferenceCollection<T> : IReadOnlyList<T>, IEquatable<ReferenceCollection<T>>
	where T : class?
{
	private Object? _references;

	internal ReferenceCollection(T reference)
	{
		_references = reference is not null ? reference : NullReference.Instance;
	}

	internal ReferenceCollection(List<T> references)
	{
		_references = references;
	}

	readonly Int32 IReadOnlyCollection<T>.Count => Count;

	internal readonly Int32 Count
	{
		get
		{
			if (_references is List<T> list)
				return list.Count;

			if (_references is not null)
				return 1;

			return 0;
		}
	}

	internal readonly T? UnderlyingReference => _references as T;

	internal readonly List<T>? UnderlyingCollection => _references as List<T>;

	readonly T IReadOnlyList<T>.this[Int32 index] => this[index];

	public readonly T this[Int32 index]
	{
		get
		{
			if (_references is T reference)
			{
				if (index == 0)
					return reference;
			}
			else if (_references is NullReference)
				return null!;
			else if (_references is not null)
				return Unsafe.As<List<T>>(_references)[index];

			return Array.Empty<T>()[0];
		}
	}

	public static implicit operator ReferenceCollection<T?>(T reference) => new(reference);

	public static implicit operator ReferenceCollection<T>(List<T> references) => new(references);

	public static Boolean operator ==(ReferenceCollection<T> left, ReferenceCollection<T> right) => left.Equals(right);

	public static Boolean operator !=(ReferenceCollection<T> left, ReferenceCollection<T> right) => !left.Equals(right);

	public readonly Enumerator GetEnumerator() => new(_references);

	public readonly Boolean Equals(ReferenceCollection<T> other)
	{
		var thisIsNull = _references is null;
		var otherIsNull = other._references is null;
		if (thisIsNull || otherIsNull)
			return thisIsNull == otherIsNull;

		var thisIsNullReference = _references is NullReference;
		var otherIsNullReference = other._references is NullReference;
		if (thisIsNullReference || otherIsNullReference)
			return thisIsNullReference == otherIsNullReference;

		if (_references is T thisReference)
		{
			if (other._references is T otherReference)
				return thisReference == otherReference;

			var otherReferences = Unsafe.As<List<T>>(other._references!);
			return otherReferences.Count == 1 && thisReference == otherReferences[0];
		}
		else
		{
			var thisReferences = Unsafe.As<List<T>>(_references!);
			if (other._references is List<T> otherReferences)
				return thisReferences.SequenceEqual(otherReferences);

			var otherReference = Unsafe.As<T>(other._references);
			return thisReferences.Count == 1 && thisReferences[0] == otherReference;
		}
	}

	public readonly override Boolean Equals(Object? obj) => obj is ReferenceCollection<T> other && Equals(other);

	public readonly override Int32 GetHashCode() => _references is not null ? _references.GetHashCode() : 0;

	readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

	readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	internal void Add(T reference)
	{
		if (_references is List<T> list)
			list.Add(reference);
		else if (_references is not null)
			_references = new List<T> { (_references as T)!, reference };
		else
			_references = reference is not null ? reference : NullReference.Instance;
	}

	internal struct Enumerator : IEnumerator<T>
	{
		private readonly List<T>? _references;
		private Object? _current;
		private Int32 _index;

		internal Enumerator(Object? reference)
		{
			if (reference is List<T> references)
			{
				_current = null;
				_references = references;
			}
			else if (reference is not null)
			{
				_current = reference;
				_references = null;
			}
			else
				_index = -1;
		}

		public readonly T Current => (_current as T)!;

		readonly Object? IEnumerator.Current => Current;

		public Boolean MoveNext()
		{
			if (_index < 0)
				return false;

			if (_references is not null)
			{
				if (_index < _references.Count)
				{
					_current = _references[_index];
					_index++;
					return true;
				}

				_current = null;
				_index = -1;
				return false;
			}

			_index = -1;
			return true;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public readonly void Dispose()
		{ }
	}

	internal sealed class NullReference
	{
		private NullReference()
		{ }

		internal static NullReference Instance { get; } = new();
	}
}
