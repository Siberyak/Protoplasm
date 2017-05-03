using System;

namespace Protoplasm.Calendars.Tests
{
    internal class ByDayOfWeekAdapter : ByDayOfWeekAdapter<int?>
    {
        public ByDayOfWeekAdapter() : base(8, 0, null)
        {
        }

        public override int? Include(int? a, int? b)
        {
            return a.HasValue || b.HasValue
                ? (a ?? 0) + (b ?? 0)
                : default(int?);
        }

        public override int? Exclude(int? a, int? b)
        {
            return a.HasValue || b.HasValue
                ? (a ?? 0) - (b ?? 0)
                : default(int?);
        }
    }
}