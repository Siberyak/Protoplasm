using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public class CalendarItem : PointedInterval<TTime, TData>, ICalendarItem
        {
            Point<TTime> ICalendarItem.Left => Left;

            Point<TTime> ICalendarItem.Right => Right;

            public TDuration? Duration => Calendars<TTime, TDuration>.ToDuration?.Invoke(Left.PointValue, Right.PointValue);

            public CalendarItem(Point<TTime> left = null, Point<TTime> right = null, TData data = default(TData)) : base(left, right, data)
            {
            }

            ICalendarItem ICalendarItem.Intersect(Point<TTime> left, Point<TTime> right)
            {
                return Intersect(left, right);
            }

            public ICalendarItem Intersect(Interval<TTime> interval)
            {
                return Intersect(interval.Left, interval.Right);
            }

            public CalendarItem Intersect(Point<TTime> left, Point<TTime> right)
            {
                var l = Left <= left ? left : Left;
                var r = Right >= right ? right : Right;
                return new CalendarItem(l, r, Data) {DataToString = DataToString};
            }

            public override string ToString()
            {
                if (!Duration.HasValue)
                    return base.ToString();

                var data = DataToString?.Invoke(Data) ?? (object) Data;
                return $"{Left}, {Right}, Duration = [{Duration}], Data = [{data}]";
            }

            bool ICalendarItem.TrySplit(Point<TTime> point, out Point<TTime>[] points)
            {
                return TrySplit(point, out points, true);
            }
        }
    }
}