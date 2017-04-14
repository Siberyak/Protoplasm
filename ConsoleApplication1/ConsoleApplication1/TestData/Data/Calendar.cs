using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using ConsoleApplication1.Intervals;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class Calendar<TData>
        {
            public delegate void DefineData(CalendarItems container, CalendarItem toDefine);

            private readonly Calendar<TData> _prev;
            private readonly DefineData _defineData;
            private readonly CalendarItems _calendarItems;

            public Calendar(DefineData defineData, CalendarItems.IncludeData includeData, CalendarItems.ExcludeData excludeData, CalendarItems.DataToString dataToString = null)
                : this(null, defineData, includeData, excludeData, dataToString)
            { }
            public Calendar(Calendar<TData> prev, DefineData defineData, CalendarItems.IncludeData includeData, CalendarItems.ExcludeData excludeData, CalendarItems.DataToString dataToString = null)
            {
                _prev = prev;
                _defineData = defineData;
                _calendarItems = new CalendarItems(includeData, excludeData, dataToString);
            }

            public IEnumerable<CalendarItem> Get(TTime from, TTime to)
            {

                List<CalendarItem> result = new List<CalendarItem>();

                var left = Point<TTime>.Left(from, false);
                var right = Point<TTime>.Right(to, false);

                SimpleLinkedList<CalendarItem>.Node node = _calendarItems.FindNode(x => x.Contains(left));

                while (node != null && node.Value.Left <= right)
                {
                    if (Equals(node.Value.Data, default(TData)))
                    {
                        var prev = node.Previous;
                        var undefinedInterval = node.Value.Intersect(left, right);
                        Define(undefinedInterval);

                        node = (prev ?? _calendarItems.FirstNode).Next;
                        if (Equals(node.Value.Data, default(TData)))
                            node = node.Next;
                    }

                    while (node != null && !Equals(node.Value.Data, default(TData)))
                    {
                        result.Add(node.Value);
                        node = node.Next;
                    }
                }

                return result;
            }

            private void Define(CalendarItem undefinedInterval)
            {
                if(undefinedInterval == null)
                    throw new ArgumentNullException(nameof(undefinedInterval));

                var intervals =
                    _prev?.Get(undefinedInterval.Left.PointValue.Value, undefinedInterval.Right.PointValue.Value)
                    ?? new[] {undefinedInterval};

                foreach (var interval in intervals)
                {
                    _defineData(_calendarItems, interval);
                }
            }

            public override string ToString()
            {
                return $"{GetType().TypeName()}";
            }

            public class CalendarItems : PointedIntervalsContainer<CalendarItem, TTime, TData> 
            {
                public CalendarItems(IncludeData includeData, ExcludeData excludeData, DataToString dataToString = null)
                    : base((left, right, data) => new CalendarItem(left, right, data), includeData, excludeData, dataToString)
                {}

                
            }

            public class CalendarItem : PointedInterval<TTime, TData> 
            {
                public CalendarItem(Point<TTime> left = null, Point<TTime> right = null, TData data = default(TData)) : base(left, right, data)
                {}

                public CalendarItem Intersect(Point<TTime> left, Point<TTime> right)
                {
                    var l = Left <= left ? left : Left;
                    var r = Right >= right ? right : Right;
                    return new CalendarItem(l, r, Data) {DataToString = DataToString};
                }
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