using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {

        public interface ICalendarItems
        {
            void Include(Point<TTime> left, Point<TTime> right, TData data);
            void Include(TTime? left, TTime? right, TData data, bool leftIncluded = true, bool rightIncluded = true);

            void Exclude(Point<TTime> left, Point<TTime> right, TData data);
            void Exclude(TTime? left, TTime? right, TData data, bool leftIncluded = true, bool rightIncluded = true);
        }
    }
}