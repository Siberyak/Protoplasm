using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Protoplasm.Utils
{
    public struct ArithmeticAdapter<T> : IEquatable<ArithmeticAdapter<T>>
        where T : IComparable<T>
    {
        public static Func<T, T, T> Add;
        public static Func<T, T, T> Subst;
        public static Func<T> ZeroValue;

        public static ArithmeticAdapter<T> Zero => new ArithmeticAdapter<T>(ZeroValue == null ? default(T) : ZeroValue(), ValueSemantic.Value);
        public static readonly ArithmeticAdapter<T> Undefined = new ArithmeticAdapter<T>(default(T), ValueSemantic.Undefined);
        public static readonly ArithmeticAdapter<T> NegativeInfinity = new ArithmeticAdapter<T>(default(T), ValueSemantic.NegativeInfinity);
        public static readonly ArithmeticAdapter<T> PositiveInfinity = new ArithmeticAdapter<T>(default(T), ValueSemantic.PositiveInfinity);



        private readonly ValueSemantic Semantic;

        public bool IsDefined => Semantic == ValueSemantic.Value;
        public bool IsInfinity => Semantic == ValueSemantic.NegativeInfinity || Semantic == ValueSemantic.NegativeInfinity;
        public bool IsNegativeInfinity => Semantic == ValueSemantic.NegativeInfinity;
        public bool IsPositiveInfinity => Semantic == ValueSemantic.PositiveInfinity;

        static ArithmeticAdapter()
        {
            ArithmeticAdapterInitializer.InitDefaults();
        }

        public override string ToString()
        {
            return $"Value: {AsString}, Type: {typeof (T).TypeName()}";
        }

        private string AsString
        {
            get
            {
                object value;
                switch (Semantic)
                {
                    case ValueSemantic.Undefined:
                        value = "<undefined>";
                        break;
                    case ValueSemantic.Value:
                        value = (object) Value ?? "null";
                        break;
                    case ValueSemantic.NegativeInfinity:
                        value = "-∞";
                        break;
                    case ValueSemantic.PositiveInfinity:
                        value = "+∞";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return $"{value}";
            }
        }

        public bool Equals(ArithmeticAdapter<T> other)
        {
            return Semantic == other.Semantic && Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ArithmeticAdapter<T> && Equals((ArithmeticAdapter<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Semantic * 397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
            }
        }



        static T AddValue(T a, T b)
        {
            if (Add == null)
                throw new DataAdapterCustomizationException(new ArgumentNullException(nameof(Add)));

            return Add(a, b);
        }

        private static ArithmeticAdapter<T> SubstValue(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b)
        {
            if (Subst == null)
                throw new DataAdapterCustomizationException(new ArgumentNullException(nameof(Subst)));
            return Subst(a.Value, b.Value);
        }

        public readonly T Value;

        private ArithmeticAdapter(T value, ValueSemantic semantic)
        {
            Value = value;
            Semantic = semantic;
        }
        

        public ArithmeticAdapter<T> Max(ArithmeticAdapter<T> other)
        {
            if (this == Undefined || other == Undefined)
                return Undefined;

            return this >= other ? this : other;
        }

        public ArithmeticAdapter<T> Min(ArithmeticAdapter<T> other)
        {
            if (this == Undefined || other == Undefined)
                return Undefined;

            return this <= other ? this : other;
        }

        public static implicit operator ArithmeticAdapter<T>(T value)
        {
            return (object)value == null ? Undefined : new ArithmeticAdapter<T>(value, ValueSemantic.Value);
        }

        public static explicit operator T(ArithmeticAdapter<T> adapter)
        {
            if (adapter.IsDefined)
                return adapter.Value;

            throw new ArgumentException($"значение адаптера не определено. {nameof(adapter)}:[{adapter}]");
        }

        public static ArithmeticAdapter<T> operator +(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b)
        {
            if (a.IsDefined && b.IsDefined)
                return AddValue(a.Value, b.Value);

            // оба не определены
            if (!a.IsDefined && !b.IsDefined) 
                return a.Equals(b) // но эквивалентны. нпример [-∞] и [-∞]
                    ? a 
                    : Undefined;

            return a.IsDefined // [v] + [-∞] = [-∞]  
                ? b
                : a;
        }

        public static ArithmeticAdapter<T> operator -(ArithmeticAdapter<T> a)
        {
            switch (a.Semantic)
            {
                case ValueSemantic.Undefined:
                    return Undefined;
                case ValueSemantic.NegativeInfinity:
                    return PositiveInfinity;
                case ValueSemantic.PositiveInfinity:
                    return NegativeInfinity;
                case ValueSemantic.Value:
                    return SubstValue(Zero, a);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ArithmeticAdapter<T> operator -(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b)
        {
            if (a.IsDefined && b.IsDefined)
                return SubstValue(a, b);

            return a + (-b);
        }

        public static bool operator ==(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b)
        {
            return !(a == b);
        }

        public static bool TryComapare(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b, out int result)
        {
            result = 0;

            if (a.Semantic == ValueSemantic.Undefined && b.Semantic == ValueSemantic.Undefined)
                return true;

            if (a.Semantic == ValueSemantic.Undefined || b.Semantic == ValueSemantic.Undefined)
                return false;

            switch (a.Semantic)
            {
                case ValueSemantic.Value:
                    switch (b.Semantic)
                    {
                        case ValueSemantic.Value:
                            result = a.Value.CompareTo(b.Value);
                            return true;
                        case ValueSemantic.NegativeInfinity:
                            result = 1;
                            return true;
                        case ValueSemantic.PositiveInfinity:
                            result = -1;
                            return true;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case ValueSemantic.PositiveInfinity:
                    if (b.Semantic != ValueSemantic.PositiveInfinity)
                        result = 1;
                    return true;

                case ValueSemantic.NegativeInfinity:
                    if (b.Semantic != ValueSemantic.NegativeInfinity)
                        result = -1;
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(a), a, null);
            }
        }

        public static bool operator >=(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b)
        {
            int result;
            return TryComapare(a, b, out result) && result >= 0;
        }

        public static bool operator <=(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b)
        {
            int result;
            return TryComapare(a, b, out result) && result <= 0;
        }

        public static bool operator >(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b)
        {
            int result;
            return TryComapare(a, b, out result) && result > 0;
        }

        public static bool operator <(ArithmeticAdapter<T> a, ArithmeticAdapter<T> b)
        {
            int result;
            return TryComapare(a, b, out result) && result < 0;
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
            public DataAdapterCustomizationException(Exception innerException) : base(typeof(ArithmeticAdapter<T>), $"Ошибка кастомизации дата-адаптера {typeof(ArithmeticAdapter<T>).TypeName()}", innerException)
            {
            }

            public DataAdapterCustomizationException(string message, Exception innerException) : base(typeof(ArithmeticAdapter<T>), message, innerException)
            {
            }

            public DataAdapterCustomizationException(string message) : base(typeof(ArithmeticAdapter<T>), message)
            {
            }
        }
    }
}