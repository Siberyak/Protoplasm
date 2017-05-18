using System;
using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public partial class Scheduler<TAmount>
            where TAmount : IComparable<TAmount>
        {

            internal struct Restrictions
            {
                public readonly Interval<TTime> Start;
                public readonly Interval<TTime> Finish;
                public readonly Interval<TDuration> TotalDuration;
                public readonly Interval<TDuration> Duration;
                public readonly TAmount RequiredAmount;

                public Point<TTime> MinStart => Start.Left;
                public Point<TTime> MaxStart => Start.Right;

                public Point<TTime> MinFinish => Finish.Left;
                public Point<TTime> MaxFinish => Finish.Right;


                public Point<TDuration> MinDuration => Duration.Left;
                public Point<TDuration> MaxDuration => Duration.Right;

                public Point<TDuration> MinTotalDuration => TotalDuration.Left;
                public Point<TDuration> MaxTotalDuration => TotalDuration.Right;


                public Restrictions(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> totalDuration, Interval<TDuration> duration, TAmount requiredAmount)
                {
                    Start = start;
                    Finish = finish;
                    TotalDuration = totalDuration;
                    Duration = duration;
                    RequiredAmount = requiredAmount;
                }
            }
        }
    }
}