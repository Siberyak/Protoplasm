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
            // ��������� ����������� �������� ��� 
            //===================================================================================
            {1, 8, 1}, //   1-8 ������
            {23, 2}, //     23 �������
            {8, 3}, //      8 �����
            {1, 5}, //      1 ��� 
            {9, 5}, //      9 ��� 
            {12, 6}, //     12 ����
            {4, 11}, //     4 ������

            //===================================================================================
            // 2017 - �������� � ����������� 
            //===================================================================================
            new DateTime(2017, 2, 24), //           � ����������� 1 ������ �� ������� 24 �������
            new DateTime(2017, 5, 8), //            � ������� 7 ������ �� ����������� 8 ���
            new DateTime(2017, 11, 6), //           � ������� 4 ������ �� ����������� 6 ������

            {new DateTime(2017, 2, 22), -1}, //     ����������� ��������������� ������� ����
            {new DateTime(2017, 3, 7), -1}, //      ����������� ��������������� ������� ����
            {new DateTime(2017, 11, 3), -1}, //     ����������� ��������������� ������� ����
            //===================================================================================
        };

        public static void WorkCalendars()
        {
            var original = Calendars<DateTime, TimeSpan>.GetOffset;

            try
            {
                // ��� �� Duration ���������� ��� ��� ������������ ������� ����� �� ������
                Calendars<DateTime, TimeSpan>.GetOffset = (a, b) => b - a;

                var byDayOfWeek = new ByDayOfWeekHelper();
                var byDayInfos = new ByDayInfosHelper(Holidays);

                // ������� ��������� ������� ���� - [��. - ��.]:(8 �.) + [��. - ����.]:(0 �.)
                var baseCalendar = new WorkCalendar(byDayOfWeek);

                // ��������� + �������� + ����������� ���������������
                var calendar = new WorkCalendar(baseCalendar, byDayInfos);

                // ����� ���������� �������
                var items1 = baseCalendar.Get(new DateTime(2017, 1, 1), DateTime.Today);
                var items2 = calendar.Get(new DateTime(2017, 1, 1), DateTime.Today);

                // ������� ��������� � ���� ������
                var result = calendar.Get(new DateTime(2017, 1, 1), new DateTime(2017, 1, 1).AddYears(1).AddDays(-1));

                // ����������� ������ ����� �� ������
                var hours1 = result.Where(x => x.Duration.HasValue).Sum(x => x.Duration.Value.Days * x.Data);

                // ������ �����
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