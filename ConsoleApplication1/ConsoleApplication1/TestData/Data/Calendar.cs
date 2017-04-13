using System;
using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {

        public class Calendar
        {
            public delegate CalendarItem GenerateCalendarItem(CalendarItem baseItem, TTime point);
            public delegate IEnumerable<CalendarItem> GenerateCalendarItems(IEnumerable<CalendarItem> baseItems);

            public GenerateCalendarItem GenerateItem;
            public GenerateCalendarItems GenerateItems;


            public IEnumerable<CalendarItem> Get(TTime left, TTime right)
            {
                return null;
            }
        }

        public class CalendarItem
        {
            public TTime Begin;
            public TTime End;
            public CalendarItemType Data;

            public override string ToString()
            {
                return $"{Begin} - {End} : {Data}";
            }
        }

       
    }

    public enum CalendarItemType
    {
        Unknown = 0,
        Available,
        Unavalable
    }
}