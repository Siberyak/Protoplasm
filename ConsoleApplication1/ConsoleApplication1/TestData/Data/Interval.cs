using System;
using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public struct Interval<T> : IEquatable<Interval<T>>
    {
        public static readonly Interval<T> Empty = new Interval<T>(default(T), default(T));

        private Interval(T begin, T end)
        {
            Begin = begin;
            End = end;
        }

        public T Begin { get; }
        public T End { get; }

        public bool Equals(Interval<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Begin, other.Begin) && EqualityComparer<T>.Default.Equals(End, other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Interval<T> && Equals((Interval<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Begin)*397) ^ EqualityComparer<T>.Default.GetHashCode(End);
            }
        }

        public static bool operator ==(Interval<T> left, Interval<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Interval<T> left, Interval<T> right)
        {
            return !left.Equals(right);
        }

        public static Interval<T> New(T value)
        {
            return new Interval<T>(value, value);
        }
        public static Interval<T> New(T begin, T end)
        {
            return new Interval<T>(begin, end);
        }
    }
}