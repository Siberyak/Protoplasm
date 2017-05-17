using System;
using System.Diagnostics;

namespace Protoplasm.Utils
{
    class DataAdapterInitializer
    {
        internal static readonly object Locker = new object();
        internal static bool DataAdapterSelfTested;
        internal static bool DataAdapterDefaultsInited;

        static void Test()
        {
            lock (Locker)
            {
                if (DataAdapterSelfTested)
                    return;

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

                DataAdapterSelfTested = true;
            }
        }

        internal static void InitDefaults()
        {
            lock (Locker)
            {
                if(DataAdapterDefaultsInited)
                {
                    return;
                }

                DataAdapter<int>.Add = (a, b) => a + b;
                DataAdapter<int>.Subst = (a, b) => a - b;

                DataAdapter<long>.Add = (a, b) => a + b;
                DataAdapter<long>.Subst = (a, b) => a - b;

                //DataAdapter<byte>.Add = (a, b) => a + b;
                //DataAdapter<byte>.Subst = (a, b) => a - b;

                //DataAdapter<short>.Add = (a, b) => a + b;
                //DataAdapter<short>.Subst = (a, b) => a - b;

                DataAdapter<double>.Add = (a, b) => a + b;
                DataAdapter<double>.Subst = (a, b) => a - b;

                DataAdapter<decimal>.Add = (a, b) => a + b;
                DataAdapter<decimal>.Subst = (a, b) => a - b;

                DataAdapter<float>.Add = (a, b) => a + b;
                DataAdapter<float>.Subst = (a, b) => a - b;

                DataAdapter<TimeSpan>.Add = (a, b) => a + b;
                DataAdapter<TimeSpan>.Subst = (a, b) => a - b;

                Test();

                DataAdapterDefaultsInited = true;
            }
        }
    }

    public struct DataAdapter<T> : IEquatable<DataAdapter<T>> 
        where T : IComparable<T>
    {
        static DataAdapter()
        {
            DataAdapterInitializer.InitDefaults();
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


        static T AddValue(T a, T b)
        {
            if (Add == null)
                throw new DataAdapterCustomizationException(new ArgumentNullException(nameof(Add)));
            return Add(a, b);
        }

        private static DataAdapter<T> SubstValue(DataAdapter<T> a, DataAdapter<T> b)
        {
            if (Subst == null)
                throw new DataAdapterCustomizationException(new ArgumentNullException(nameof(Subst)));
            return Subst(a.Value, b.Value);
        }

        public readonly T Value;
        public DataAdapter(T value)
        {
            Value = value;
        }

        public DataAdapter<T> Max(DataAdapter<T> other)
        {
            return this >= other ? this : other;
        }
        public DataAdapter<T> Min(DataAdapter<T> other)
        {
            return this <= other ? this : other;
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
            return AddValue(a.Value, b.Value);
        }

        public static DataAdapter<T> operator -(DataAdapter<T> a, DataAdapter<T> b)
        {
            return SubstValue(a, b);
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

        public static T Min(T a, T b)
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }

        public static T Max(T a, T b)
        {
            return a.CompareTo(b) >= 0 ? a : b;
        }

        public class DataAdapterCustomizationException : CustomizationException
        {
            public DataAdapterCustomizationException(Exception innerException)
                : base(typeof(DataAdapter<T>), $"Ошибка кастомизации дата-адаптера {typeof(DataAdapter<T>).TypeName()}", innerException)
            {
            }

            public DataAdapterCustomizationException(string message, Exception innerException)
                : base(typeof(DataAdapter<T>), message, innerException)
            {
            }

            public DataAdapterCustomizationException(string message)
                : base(typeof(DataAdapter<T>), message)
            {
            }
        }

    }
}