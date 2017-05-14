using System.Collections.Generic;
using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public interface ICalendarItem
        {
            bool IsDefined { get; }
            bool IsUndefined { get; }
            bool IsPartialDefined { get; }

            Point<TTime> Left { get; }
            Point<TTime> Right { get; }
            TDuration? Duration { get; }
            TData Data { get; }

            bool Contains(Point<TTime> point);

            bool TrySplit(Point<TTime> point, out Point<TTime>[] points);

            ICalendarItem Intersect(Point<TTime> left, Point<TTime> right);
            ICalendarItem Intersect(Interval<TTime> interval);
        }
    }
}