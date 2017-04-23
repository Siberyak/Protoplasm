using System;

namespace Protoplasm.Calendars
{
    public static class Calendars<TTime, TDuration>
        where TTime : struct, IComparable<TTime> where TDuration : struct, IComparable<TDuration>
    {
        public static Func<TTime?, TTime?, TDuration?> GetOffset;

        public static TTime Min(TTime a, TTime b)
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }
        public static TTime Max(TTime a, TTime b)
        {
            return a.CompareTo(b) >= 0 ? a : b;
        }

    }
}