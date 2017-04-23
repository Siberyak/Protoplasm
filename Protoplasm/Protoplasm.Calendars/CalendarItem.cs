using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {


        public class CalendarItem : PointedInterval<TTime, TData>
        {
            public TDuration? Duration => Calendars<TTime, TDuration>.GetOffset?.Invoke(Left.PointValue, Right.PointValue);

            public CalendarItem(Point<TTime> left = null, Point<TTime> right = null, TData data = default(TData)) : base(left, right, data)
            {
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
        }
    }
}