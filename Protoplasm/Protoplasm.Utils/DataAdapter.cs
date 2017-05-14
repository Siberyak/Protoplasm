using System;
using System.Diagnostics;

namespace Protoplasm.Utils
{
    public struct DataAdapter<T> : IEquatable<DataAdapter<T>>
        where T : IComparable<T>
    {
        private static bool _selfTested;

        static DataAdapter()
        {
            if (_selfTested)
                return;

            DataAdapter<int>.Add = (a, b) => a + b;
            DataAdapter<int>.Subst = (a, b) => a - b;

            DataAdapter<int> one = 1;
            DataAdapter<int> zerro = 0;

            Debug.Assert(zerro + one == 1);
            Debug.Assert(zerro + one == one);

            Debug.Assert(one > 0);
            Debug.Assert(one >= 0);
            Debug.Assert(one != 0);
            Debug.Assert(!(one == 0));
            Debug.Assert(!(one <= 0));
            Debug.Assert(!(one < 0));

            DataAdapter<int> minus_one;
            Debug.Assert((minus_one = one - 2) == -1);
            Debug.Assert(!(minus_one > 0));
            Debug.Assert(!(minus_one >= 0));
            Debug.Assert(minus_one != 0);
            Debug.Assert(!(minus_one == 0));
            Debug.Assert(minus_one <= 0);
            Debug.Assert(minus_one < 0);


            Debug.Assert(!(minus_one + one > 0));
            Debug.Assert(minus_one + one >= 0);
            Debug.Assert(!(minus_one + one != 0));
            Debug.Assert(minus_one + one == 0);
            Debug.Assert(minus_one + one <= 0);
            Debug.Assert(!(minus_one + one < 0));

            Debug.Assert(!(minus_one + one > zerro));
            Debug.Assert(minus_one + one >= zerro);
            Debug.Assert(!(minus_one + one != zerro));
            Debug.Assert(minus_one + one == zerro);
            Debug.Assert(minus_one + one <= zerro);
            Debug.Assert(!(minus_one + one < zerro));

            _selfTested = true;
        }

        public override string ToString()
        {
            var value = (object)Value ?? "null";
            return $"Value: {value}, Type: {typeof(T).TypeName()}";
        }

        public bool Equals(DataAdapter<T> other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DataAdapter<T> && Equals((DataAdapter<T>)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static Func<T, T, T> Add;
        public static Func<T, T, T> Subst;

        public readonly T Value;
        public DataAdapter(T value)
        {
            Value = value;
        }

        public static implicit operator DataAdapter<T>(T value)
        {
            return !Equals(value, default(T)) ? new DataAdapter<T>(value) : default(DataAdapter<T>);
        }

        public static explicit operator T(DataAdapter<T> adapter)
        {
            return !Equals(adapter, default(DataAdapter<T>)) ? adapter.Value : default(T);
        }

        public static DataAdapter<T> operator +(DataAdapter<T> a, DataAdapter<T> b)
        {
            return Add(a.Value, b.Value);
        }
        public static DataAdapter<T> operator -(DataAdapter<T> a, DataAdapter<T> b)
        {
            return Subst(a.Value, b.Value);
        }

        public static bool operator ==(DataAdapter<T> a, DataAdapter<T> b)
        {
            return a.Value.CompareTo(b.Value) == 0;
        }

        public static bool operator !=(DataAdapter<T> a, DataAdapter<T> b)
        {
            return !(a == b);
        }

        public static bool operator >=(DataAdapter<T> a, DataAdapter<T> b)
        {
            return a.Value.CompareTo(b.Value) >= 0;
        }

        public static bool operator <=(DataAdapter<T> a, DataAdapter<T> b)
        {
            return a.Value.CompareTo(b.Value) <= 0;
        }

        public static bool operator >(DataAdapter<T> a, DataAdapter<T> b)
        {
            return a.Value.CompareTo(b.Value) > 0;
        }

        public static bool operator <(DataAdapter<T> a, DataAdapter<T> b)
        {
            return a.Value.CompareTo(b.Value) < 0;
        }

    }
}