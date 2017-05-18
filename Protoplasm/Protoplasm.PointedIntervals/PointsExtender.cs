using System;
using Protoplasm.Utils;

namespace Protoplasm.PointedIntervals
{
    public static class PointsExtender
    {
        public static Interval<TBound> Interval<TBound>(this Point<TBound> left, Point<TBound> right)
            where TBound : struct, IComparable<TBound>
        {
            return new Interval<TBound>(left, right);
        }

        public static Point<TBound> Min<TBound>(this Point<TBound> a, Point<TBound> b) 
            where TBound : struct, IComparable<TBound>
        {
            return a > b ? b : a;
        }
        public static Point<TBound> Max<TBound>(this Point<TBound> a, Point<TBound> b)
            where TBound : struct, IComparable<TBound>
        {
            return a < b ? b : a;
        }

        public static Point<TBound> Left<TBound>(this ArithmeticAdapter<TBound>? value, bool included = true)
            where TBound : struct, IComparable<TBound>
        {
            return Point<TBound>.Left(value?.Value, included);
        }
        public static Point<TBound> Right<TBound>(this ArithmeticAdapter<TBound>? value, bool included = true)
            where TBound : struct, IComparable<TBound>
        {
            return Point<TBound>.Right(value?.Value, included);
        }

        public static Point<TBound> Left<TBound>(this ArithmeticAdapter<TBound> value, bool included = true)
            where TBound : struct, IComparable<TBound>
        {
            return Point<TBound>.Left(value.Value, included);
        }
        public static Point<TBound> Right<TBound>(this ArithmeticAdapter<TBound> value, bool included = true)
            where TBound : struct, IComparable<TBound>
        {
            return Point<TBound>.Right(value.Value, included);
        }

        public static Point<TBound> Left<TBound>(this TBound value, bool included = true)
           where TBound : struct, IComparable<TBound>
        {
            return Point<TBound>.Left(value, included);
        }
        public static Point<TBound> Right<TBound>(this TBound value, bool included = true)
            where TBound : struct, IComparable<TBound>
        {
            return Point<TBound>.Right(value, included);
        }

        public static Point<TBound> Left<TBound>(this TBound? value, bool included = true)
           where TBound : struct, IComparable<TBound>
        {
            return Point<TBound>.Left(value, included);
        }
        public static Point<TBound> Right<TBound>(this TBound? value, bool included = true)
            where TBound : struct, IComparable<TBound>
        {
            return Point<TBound>.Right(value, included);
        }
    }
}