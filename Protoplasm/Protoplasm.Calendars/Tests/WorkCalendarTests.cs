using System;
using System.Diagnostics;
using System.Linq;

namespace Protoplasm.Calendars.Tests
{
    public class WorkCalendarTests
    {
        private static readonly Holidays Holidays = new Holidays
        {
            //===================================================================================
            // ежегодные праздничные выходные дни 
            //===================================================================================
            {1, 8, 1}, //   1-8 €нвар€
            {23, 2}, //     23 феврал€
            {8, 3}, //      8 марта
            {1, 5}, //      1 ма€ 
            {9, 5}, //      9 ма€ 
            {12, 6}, //     12 июн€
            {4, 11}, //     4 но€бр€

            //===================================================================================
            // 2017 - переносы и сокращенные 
            //===================================================================================
            new DateTime(2017, 2, 24), //           с воскресень€ 1 €нвар€ на п€тницу 24 феврал€
            new DateTime(2017, 5, 8), //            с субботы 7 €нвар€ на понедельник 8 ма€
            new DateTime(2017, 11, 6), //           с субботы 4 но€бр€ на понедельник 6 но€бр€

            {new DateTime(2017, 2, 22), -1}, //     сокращенный предпраздничный рабочий день
            {new DateTime(2017, 3, 7), -1}, //      сокращенный предпраздничный рабочий день
            {new DateTime(2017, 11, 3), -1}, //     сокращенный предпраздничный рабочий день
            //===================================================================================
        };

        public static void WorkCalendars()
        {
            var original = Calendars<DateTime, TimeSpan>.GetOffset;

            try
            {
                // что бы Duration вычисл€лс€ сам дл€ контрольного расчета часов по списку
                Calendars<DateTime, TimeSpan>.GetOffset = (a, b) => b - a;

                var byDayOfWeek = new ByDayOfWeekHelper();
                var byDayInfos = new ByDayInfosHelper(Holidays);

                // базовый календарь рабочих дней - [пн. - пт.]:(8 ч.) + [сб. - вскр.]:(0 ч.)
                var baseCalendar = new WorkCalendar(byDayOfWeek);

                // праздники + переносы + укороченные предпраздничные
                var calendar = new WorkCalendar(baseCalendar, byDayInfos);

                // чисто посмотреть разницу
                var items1 = baseCalendar.Get(new DateTime(2017, 1, 1), DateTime.Today);
                var items2 = calendar.Get(new DateTime(2017, 1, 1), DateTime.Today);

                // искомый результат в виде списка
                var result = calendar.Get(new DateTime(2017, 1, 1), new DateTime(2017, 1, 1).AddYears(1).AddDays(-1));

                // контрольный расчет часов по списку
                var hours1 = result.Where(x => x.Duration.HasValue).Sum(x => x.Duration.Value.Days * x.Data);

                // расчет часов
                var hours2 = calendar.WorkHours(2017);

                Debug.Assert(hours1 == hours2);
                Debug.Assert(hours2 == 1973);
            }
            finally
            {
                Calendars<DateTime, TimeSpan>.GetOffset = original;
            }

        }
    }
}