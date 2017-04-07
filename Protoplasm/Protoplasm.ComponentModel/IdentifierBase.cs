using System;

namespace Protoplasm.ComponentModel
{
	public interface IIdentifier : IEquatable<IIdentifier>
	{}

	public abstract class IdentifierBase : IIdentifier
	{
		protected abstract bool Equals(IdentifierBase other);

		bool IEquatable<IIdentifier>.Equals(IIdentifier other)
		{
			return other != null && Equals(other);
		}

		public override bool Equals(object obj)
		{
			return ((IIdentifier)this).Equals(obj as IIdentifier);
		}

		public static implicit operator IdentifierBase(string id)
		{
			return new StringIdentifier(id);
		}
	}

    public class GenericIdentifier<T> : IdentifierBase
    {
        public GenericIdentifier(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }

        protected override bool Equals(IdentifierBase other)
        {
            var identifier = other as GenericIdentifier<T>;
            return identifier != null && Equals(identifier.Value, Value);
        }

        public override string ToString()
        {
            var type = typeof(T);
            return string.Format("{0}.{1}: [{2}]", type.Namespace, type.Name, Value);
        }
    }

    public class GenericIdentifier<T1, T2> : IdentifierBase
    {
        public GenericIdentifier(T1 part1, T2 part2)
        {
            Part1 = part1;
            Part2 = part2;
            //if (Equals(value, default(T)))
            //    throw new ArgumentException("value has default value", "value");
        }

        public T1 Part1 { get; private set; }
        public T2 Part2 { get; private set; }

        protected override bool Equals(IdentifierBase other)
        {
            var identifier = other as GenericIdentifier<T1, T2>;
            return identifier != null && Equals(identifier.Part1, Part1) && Equals(identifier.Part2, Part2);
        }

        public override string ToString()
        {
            var part1Type = typeof(T1);
            var part2Type = typeof(T2);
            var p1 = string.Format("{0}.{1}: [{2}]", part1Type.Namespace, part1Type.Name, Part1);
            var p2 = string.Format("{0}.{1}: [{2}]", part2Type.Namespace, part2Type.Name, Part2);
            return string.Format("({0}, {1})", p1, p2);
        }
    }
}