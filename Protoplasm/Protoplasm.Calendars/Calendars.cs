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
            if (DataAdapter<TDuration>.Add == null)
                DataAdapter<TDuration>.Add = Add;
            else
                AddDuration = DataAdapter<TDuration>.Add;

            if (DataAdapter<TDuration>.Subst == null)
                DataAdapter<TDuration>.Subst = Subst;
            else
                SubstDuration = DataAdapter<TDuration>.Subst;
        }

        public static Func<TTime?, TTime?, TDuration?> ToDuration;
        public static Func<TTime?, TDuration?, TTime?> OffsetToRight;
        public static Func<TTime?, TDuration?, TTime?> OffsetToLeft;
        public static Func<TDuration, TDuration, TDuration> AddDuration;
        public static Func<TDuration, TDuration, TDuration> SubstDuration;


        public static TTime Min(TTime a, TTime b)
        {
            return DataAdapter<TTime>.Min(a, b);
        }
        public static TTime Max(TTime a, TTime b)
        {
            return DataAdapter<TTime>.Max(a, b);
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
            return DataAdapter<TDuration>.Min(a, b);
        }
        public static TDuration Max(TDuration a, TDuration b)
        {
            return DataAdapter<TDuration>.Max(a, b);
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
                ? DataAdapter<T>.Min(a.Value, b.Value)
                : a.HasValue ? a : b;
        }

        private static T? Max<T>(T? a, T? b)
            where T : struct, IComparable<T>
        {
            return a.HasValue && b.HasValue
                ? DataAdapter<T>.Max(a.Value, b.Value)
                : a.HasValue ? a : b;
        }

        public static TDuration? Duration(TTime? a, TTime? b)
        {
            if(ToDuration == null)
                throw new CalendarsCustomizationException(new ArgumentNullException(nameof(ToDuration)));
            return ToDuration(a, b);
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
                : base(typeof(Calendars<TTime, TDuration>), $"������ ������������ ��������� {typeof(Calendars<TTime, TDuration>).TypeName()}", innerException)
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


}