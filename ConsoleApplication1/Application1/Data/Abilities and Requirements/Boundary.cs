using System;
using System.Linq;
using Protoplasm.Calendars;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public struct Boundary
    {
        public readonly Interval<DateTime> Start;
        public readonly Interval<DateTime> Finish;
        public readonly Interval<TimeSpan> TotalDuration;
        public readonly Interval<TimeSpan> Duration;

        private Boundary(Interval<DateTime> start, Interval<DateTime> finish, Interval<TimeSpan> totalDuration, Interval<TimeSpan> duration)
        {
            Start = start.Instance();
            Finish = finish.Instance();
            TotalDuration = totalDuration.Instance();
            Duration = duration.Instance();
        }

        public bool IsCorrect
        {
            get
            {
                return Start.Left < Finish.Right;
            }
        }

        public override string ToString()
        {
            var start = Start.IsUndefined ? null : $"S: {Start}";
            var finish = Finish.IsUndefined ? null : $"F: {Finish}";
            var duration = Duration.IsUndefined ? null : $"D: {Duration}";
            var totalDuration = TotalDuration.IsUndefined ? null : $"TD: {TotalDuration}";
            var parts = new[] { start, finish, duration, totalDuration };
            return $"Boundary: [{string.Join(", ", parts.Where(x => x != null))}]";
        }

        public static readonly Boundary Empty = default(Boundary);

        public Boundary StartAfter(DateTime value, bool keepIntervalDuration = true, bool included = true)
        {
            var interval = Start.ChangeLeft<DateTime, TimeSpan>(value, keepIntervalDuration, included);
            return new Boundary(interval, Finish, TotalDuration, Duration);
        }

        public Boundary StartBefore(DateTime value, bool keepIntervalDuration = true, bool included = true)
        {
            var interval = Start.ChangeRight<DateTime, TimeSpan>(value, keepIntervalDuration, included);
            return new Boundary(interval, Finish, TotalDuration, Duration);
        }

        public Boundary FinishAfter(DateTime value, bool keepIntervalDuration = true, bool included = true)
        {
            var interval = Finish.ChangeLeft<DateTime, TimeSpan>(value, keepIntervalDuration, included);
            return new Boundary(Start, interval, TotalDuration, Duration);
        }

        public Boundary FinishBefore(DateTime value, bool keepIntervalDuration = true, bool included = true)
        {
            var interval = Finish.ChangeRight<DateTime, TimeSpan>(value, keepIntervalDuration, included);
            return new Boundary(Start, interval, TotalDuration, Duration);
        }

        public Boundary NotLonger(TimeSpan value, bool keepIntervalDuration = true, bool included = true)
        {
            var interval = Duration.ChangeRight<TimeSpan, TimeSpan>(value, keepIntervalDuration, included);
            return new Boundary(Start, Finish, TotalDuration, interval);
        }

        public Boundary TotalNotLonger(TimeSpan value, bool keepIntervalDuration = true, bool included = true)
        {
            var interval = TotalDuration.ChangeRight<TimeSpan, TimeSpan>(value, keepIntervalDuration, included);
            return new Boundary(Start, Finish, interval, Duration);
        }

        public Boundary NotShorter(TimeSpan value, bool keepIntervalDuration = true, bool included = true)
        {
            var interval = Duration.ChangeLeft<TimeSpan, TimeSpan>(value, keepIntervalDuration, included);
            return new Boundary(Start, Finish, TotalDuration, interval);
        }

        public Boundary TotalNotShorter(TimeSpan value, bool keepIntervalDuration = true, bool included = true)
        {
            var interval = TotalDuration.ChangeLeft<TimeSpan, TimeSpan>(value, keepIntervalDuration, included);
            return new Boundary(Start, Finish, interval, Duration);
        }

        public Boundary Restrict(Boundary restriction)
        {
            var start = restriction.Start.Intersect(Start);
            var finish = restriction.Finish.Intersect(Finish);
            var totalDuration = restriction.TotalDuration.Intersect(TotalDuration);
            var duration = restriction.Duration.Intersect(Duration);

            return new Boundary(start, finish, totalDuration, duration);
        }
    }
}