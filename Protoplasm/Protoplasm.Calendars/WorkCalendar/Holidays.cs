using System;
using System.Collections;
using System.Collections.Generic;
using Protoplasm.Calendars.Tests;

namespace Protoplasm.Calendars
{
    public class Holidays : IEnumerable<DayInfoBase>
    {
        private readonly List<DayInfoBase> _holidays = new List<DayInfoBase>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<DayInfoBase> GetEnumerator()
        {
            return _holidays.GetEnumerator();
        }

        /// <summary>
        /// Для ежегодных выходных.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="month"></param>
        public void Add(int day, int month)
        {
            _holidays.Add(new YearlyHoliday(day, month));
        }

        /// <summary>
        /// Для диапазонов ежегодных выходных. Например, 1-8 января
        /// </summary>
        /// <param name="dayfrom"></param>
        /// <param name="dayTo"></param>
        /// <param name="month"></param>
        public void Add(int dayfrom, int dayTo, int month)
        {
            _holidays.AddRange(YearlyHoliday.Range(dayfrom, dayTo, month));
        }

        /// <summary>
        /// Для перенесенных выходных.
        /// </summary>
        /// <param name="date"></param>
        public void Add(DateTime date)
        {
            _holidays.Add(new DayInfo(date, false));
        }

        /// <summary>
        /// Для рабочих дней с отличной от базовой длительностью. Например для предпраздничных дней
        /// </summary>
        /// <param name="date"></param>
        /// <param name="difference"></param>
        public void Add(DateTime date, int difference)
        {
            _holidays.Add(new DayInfo(date, TimeSpan.FromHours(difference)));
        }
    }
}