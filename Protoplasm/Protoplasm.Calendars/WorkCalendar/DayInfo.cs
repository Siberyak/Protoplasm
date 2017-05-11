using System;
using Protoplasm.Calendars.Tests;

namespace Protoplasm.Calendars
{
    public class DayInfo : DayInfoBase
    {
        public override TimeSpan Difference => _difference ?? base.Difference;

        public readonly DateTime Date;

        private readonly TimeSpan? _difference;
        public override bool IsWorkday { get; }

        public DayInfo(DateTime date, TimeSpan difference) : this(date, true)
        {
            _difference = difference;
        }

        public DayInfo(DateTime date, bool isWorkday)
        {
            Date = date;
            IsWorkday = isWorkday;
        }

        public override bool Between(DateTime left, DateTime right)
        {
            return left.Date <= Date && Date <= right.Date;
        }
    }
}