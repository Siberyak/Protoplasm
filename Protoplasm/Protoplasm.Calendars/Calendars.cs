using System;
using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
        where TTime : struct, IComparable<TTime>
        where TDuration : struct, IComparable<TDuration>
    {
    }

    public static class Calendars<TTime, TDuration>
        where TTime : struct, IComparable<TTime> 
        where TDuration : struct, IComparable<TDuration>
    {
        public static Func<TTime?, TTime?, TDuration?> ToDuration;
        public static Func<TTime?, TDuration?, TTime?> OffsetToRight;
        public static Func<TTime?, TDuration?, TTime?> OffsetToLeft;

        public static TTime Min(TTime a, TTime b)
        {
            return Min<TTime>(a, b);
        }
        public static TTime Max(TTime a, TTime b)
        {
            return Min<TTime>(a, b);
        }
        public static TTime? Min(TTime? a, TTime? b)
        {
            return Min<TTime>(a, b);
        }
        public static TTime? Max(TTime? a, TTime? b)
        {
            return Min<TTime>(a, b);
        }

        public static TDuration Min(TDuration a, TDuration b)
        {
            return Min<TDuration>(a, b);
        }
        public static TDuration Max(TDuration a, TDuration b)
        {
            return Max<TDuration>(a, b);
        }

        public static TDuration? Min(TDuration? a, TDuration? b)
        {
            return Min<TDuration>(a, b);
        }
        public static TDuration? Max(TDuration? a, TDuration? b)
        {
            return Max<TDuration>(a, b);
        }


        private static T Min<T>(T a, T b)
            where T : IComparable<T>
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }

        private static T Max<T>(T a, T b)
            where T : IComparable<T>
        {
            return a.CompareTo(b) >= 0 ? a : b;
        }

        private static T? Min<T>(T? a, T? b)
            where T : struct, IComparable<T>
        {
            return a.HasValue && b.HasValue
                ? Min(a.Value, b.Value)
                : a.HasValue ? a : b;
        }

        private static T? Max<T>(T? a, T? b)
            where T : struct , IComparable<T>
        {
            return a.HasValue && b.HasValue
                ? Max(a.Value, b.Value)
                : a.HasValue ? a : b;
        }


        public static Point<TTime> OffsetPointToLeft(Point<TTime> point, TDuration offset)
        {
            var value = OffsetToLeft(point.PointValue, offset);
            return CreateOffsetedPoint(point, value);
        }
        public static Point<TTime> OffsetPointToRight(Point<TTime> point, TDuration offset)
        {
            var value = OffsetToRight(point.PointValue, offset);
            return CreateOffsetedPoint(point, value);
        }

        private static Point<TTime> CreateOffsetedPoint(Point<TTime> point, TTime? value)
        {
            return point.Direction == PointDirection.Left
                ? Point<TTime>.Left(value, point.Included)
                : Point<TTime>.Right(value, point.Included);
        }

        public class Sch<TC, TSch>
        {
            private readonly Calendars<TTime, TDuration, TC>.MutableCalendar _available;
            private readonly Calendars<TTime, TDuration, TSch>.CalendarItems _allocated;

            
        }
    }
}