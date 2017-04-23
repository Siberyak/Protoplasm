using System;

namespace Protoplasm.Calendars.Tests
{
    public abstract class DayInfoBase
    {
        public virtual TimeSpan Difference { get; } = TimeSpan.Zero;
        public abstract bool IsWorkday { get; }
        public abstract bool Between(DateTime left, DateTime right);

    }
}