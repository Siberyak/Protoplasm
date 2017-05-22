using System;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

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
        static Calendars()
        {
            if (ArithmeticAdapter<TDuration>.Add == null)
                ArithmeticAdapter<TDuration>.Add = Add;
            else
                AddDuration = ArithmeticAdapter<TDuration>.Add;

            if (ArithmeticAdapter<TDuration>.Subst == null)
                ArithmeticAdapter<TDuration>.Subst = Subst;
            else
                SubstDuration = ArithmeticAdapter<TDuration>.Subst;
        }

        public static Func<TTime?, TTime?, TDuration?> ToDuration;
        public static Func<TTime?, TDuration?, TTime?> OffsetToRight;
        public static Func<TTime?, TDuration?, TTime?> OffsetToLeft;
        public static Func<TDuration, TDuration, TDuration> AddDuration;
        public static Func<TDuration, TDuration, TDuration> SubstDuration;


        public static TTime Min(TTime a, TTime b)
        {
            return ArithmeticAdapter<TTime>.Min(a, b);
        }
        public static TTime Max(TTime a, TTime b)
        {
            return ArithmeticAdapter<TTime>.Max(a, b);
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
            return ArithmeticAdapter<TDuration>.Min(a, b);
        }
        public static TDuration Max(TDuration a, TDuration b)
        {
            return ArithmeticAdapter<TDuration>.Max(a, b);
        }

        public static TDuration? Min(TDuration? a, TDuration? b)
        {
            return Min<TDuration>(a, b);
        }
        public static TDuration? Max(TDuration? a, TDuration? b)
        {
            return Max<TDuration>(a, b);
        }


        private static T? Min<T>(T? a, T? b)
            where T : struct, IComparable<T>
        {
            return a.HasValue && b.HasValue
                ? ArithmeticAdapter<T>.Min(a.Value, b.Value)
                : a.HasValue ? a : b;
        }

        private static T? Max<T>(T? a, T? b)
            where T : struct, IComparable<T>
        {
            return a.HasValue && b.HasValue
                ? ArithmeticAdapter<T>.Max(a.Value, b.Value)
                : a.HasValue ? a : b;
        }

        public static TDuration? Duration(TTime? a, TTime? b)
        {
            if(ToDuration == null)
                throw new CalendarsCustomizationException(new ArgumentNullException(nameof(ToDuration)));
            return ToDuration(a, b);
        }

        public static TDuration Duration(Interval<TTime> interval)
        {
            return Duration(interval.Left, interval.Right);
        }

        public static TDuration Duration(Point<TTime> left, Point<TTime> right)
        {
            var duration = Duration((TTime?)left, right);
            if (!duration.HasValue)
                throw new NotSupportedException("не удалось вычислить длительность");
            return duration.Value;
        }

        public static TDuration Add(TDuration a, TDuration b)
        {
            if (AddDuration == null)
                throw new CalendarsCustomizationException(new ArgumentNullException(nameof(AddDuration)));

            return AddDuration(a, b);
        }

        public static TDuration? Add(TDuration? a, TDuration? b)
        {
            return a.HasValue && b.HasValue
                ? Add(a.Value, b.Value)
                : a.HasValue
                    ? a : b;
        }

        public class CalendarsCustomizationException : CustomizationException
        {
            public CalendarsCustomizationException(Exception innerException)
                : base(typeof(Calendars<TTime, TDuration>), $"Ошибка кастомизации календаря {typeof(Calendars<TTime, TDuration>).TypeName()}", innerException)
            {
            }

            public CalendarsCustomizationException(string message, Exception innerException)
                : base(typeof(Calendars<TTime, TDuration>), message, innerException)
            {
            }

            public CalendarsCustomizationException(string message)
                : base(typeof(Calendars<TTime, TDuration>), message)
            {
            }
        }

        public static TDuration Subst(TDuration a, TDuration b)
        {
            if (SubstDuration == null)
                throw new CalendarsCustomizationException(new ArgumentNullException(nameof(SubstDuration)));

            return SubstDuration(a, b);
        }

        public static TDuration? Subst(TDuration? a, TDuration? b)
        {
            return a.HasValue && b.HasValue
                ? Subst(a.Value, b.Value)
                : b.HasValue
                    ? Subst(default(TDuration), b.Value)
                    : a;
        }


        public static Interval<TTime> ChangeLeft(Interval<TTime> original, Point<TTime> left, bool keepIntervalDuration)
        {
            left = left.AsLeft();

            keepIntervalDuration = keepIntervalDuration && !left.IsUndefined;
            original = original ?? Interval<TTime>.Undefined;
            Point<TTime> right;

            if (original.IsUndefined || original.Right.IsUndefined || original.Right < left)
            {
                right = Point<TTime>.Right();
            }
            else if (keepIntervalDuration)
            {
                var duration = Duration(original);
                var point = left.OffsetToRight(duration);
                right = point.AsRight(original.Right.Included);
            }
            else
                right = original.Right;

            var interval = left.Interval(right);
            return interval;
        }

        public static Interval<TTime> ChangeRight(Interval<TTime> original, Point<TTime> right, bool keepIntervalDuration)
        {
            right = right.AsRight();
            keepIntervalDuration = keepIntervalDuration && !right.IsUndefined;
            original = original ?? Interval<TTime>.Undefined;
            Point<TTime> left;

            if (original.IsUndefined || original.Left.IsUndefined || original.Left > right)
            {
                left = Point<TTime>.Left();
            }
            else if (keepIntervalDuration)
            {
                var duration = Duration(original);
                var point = right.OffsetToLeft(duration);
                left = point.AsLeft(original.Left.Included);
            }
            else
                left = original.Left;

            var interval = left.Interval(right);
            return interval;
        }

        public static Point<TTime> OffsetPointToLeft(Point<TTime> point, TDuration offset)
        {
            if (OffsetToLeft == null)
                throw new CalendarsCustomizationException(new ArgumentNullException(nameof(OffsetToLeft)));

            var value = OffsetToLeft(point.PointValue, offset);
            return CreateOffsetedPoint(point, value);
        }
        public static Point<TTime> OffsetPointToRight(Point<TTime> point, TDuration offset)
        {
            if (OffsetToRight == null)
                throw new CalendarsCustomizationException(new ArgumentNullException(nameof(OffsetToRight)));

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

    public static class CalendarsExtender
    {
        public static Point<TTime> OffsetToLeft<TTime, TDuration>(this Point<TTime> point, ArithmeticAdapter<TDuration> offset)
            where TTime : struct, IComparable<TTime>
            where TDuration : struct, IComparable<TDuration>
        {
            return offset == default(ArithmeticAdapter<TDuration>)
                ? point
                : Calendars<TTime, TDuration>.OffsetPointToLeft(point, offset.Value);
        }

        public static Point<TTime> OffsetToRight<TTime, TDuration>(this Point<TTime> point, ArithmeticAdapter<TDuration> offset)
            where TTime : struct, IComparable<TTime>
            where TDuration : struct, IComparable<TDuration>
        {
            return offset == default(ArithmeticAdapter<TDuration>) 
                ? point 
                : Calendars<TTime, TDuration>.OffsetPointToRight(point, offset.Value);
        }

        public static Point<TTime> OffsetToLeft<TTime, TDuration>(this Point<TTime> point, TDuration offset)
            where TTime : struct, IComparable<TTime>
            where TDuration : struct, IComparable<TDuration>
        {
            return Calendars<TTime, TDuration>.OffsetPointToLeft(point, offset);
        }
        public static Point<TTime> OffsetToRight<TTime, TDuration>(this Point<TTime> point, TDuration offset)
            where TTime : struct, IComparable<TTime>
            where TDuration : struct, IComparable<TDuration>
        {
            return Calendars<TTime, TDuration>.OffsetPointToRight(point, offset);
        }

        public static Interval<TTime> ChangeLeft<TTime, TDuration>(this Interval<TTime> original, TTime value, bool keepIntervalDuration, bool included)
            where TTime : struct, IComparable<TTime>
            where TDuration : struct, IComparable<TDuration>
        {
            return original.ChangeLeft<TTime, TDuration>(value.Left(included), keepIntervalDuration);
        }

        public static Interval<TTime> ChangeLeft<TTime, TDuration>(this Interval<TTime> original, Point<TTime> left, bool keepIntervalDuration)
            where TTime : struct, IComparable<TTime>
            where TDuration : struct, IComparable<TDuration>
        {
            return Calendars<TTime, TDuration>.ChangeLeft(original, left, keepIntervalDuration);
        }

        public static Interval<TTime> ChangeRight<TTime, TDuration>(this Interval<TTime> original, TTime value, bool keepIntervalDuration, bool included)
            where TTime : struct, IComparable<TTime>
            where TDuration : struct, IComparable<TDuration>
        {
            return original.ChangeRight<TTime, TDuration>(value.Right(included), keepIntervalDuration);
        }

        public static Interval<TTime> ChangeRight<TTime, TDuration>(this Interval<TTime> original, Point<TTime> right, bool keepIntervalDuration)
            where TTime : struct, IComparable<TTime>
            where TDuration : struct, IComparable<TDuration>
        {
            return Calendars<TTime, TDuration>.ChangeRight(original, right, keepIntervalDuration);
        }

    }

}