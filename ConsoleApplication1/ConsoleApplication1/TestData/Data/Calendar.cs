using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

namespace ConsoleApplication1.TestData
{
    public interface IAbstractCalendar
    {
        IAbstractCalendar[] FullChain();

    }

    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public static class Calendars<TData>
        {

            public abstract class HelperBase : PrevBasedCalendar<TData>.HelperBase
            { }

            public interface ICalendar : IAbstractCalendar
            {
                IEnumerable<CalendarItem> Get(TTime from, TTime to);
            }

            public class CalendarItems : PointedIntervalsContainer<CalendarItem, TTime, TData>
            {
                public CalendarItems(IncludeData includeData, ExcludeData excludeData, DataToString dataToString = null)
                    : base((left, right, data) => new CalendarItem(left, right, data), includeData, excludeData, dataToString)
                { }


            }

            public class CalendarItem : PointedInterval<TTime, TData>
            {
                public TDuration? Duration => GetOffset?.Invoke(Left.PointValue, Right.PointValue);

                public CalendarItem(Point<TTime> left = null, Point<TTime> right = null, TData data = default(TData)) : base(left, right, data)
                { }

                public CalendarItem Intersect(Point<TTime> left, Point<TTime> right)
                {
                    var l = Left <= left ? left : Left;
                    var r = Right >= right ? right : Right;
                    return new CalendarItem(l, r, Data) { DataToString = DataToString };
                }

                public override string ToString()
                {
                    if (!Duration.HasValue)
                        return base.ToString();

                    var data = DataToString?.Invoke(Data) ?? (object)Data;
                    return $"{Left}, {Right}, Duration = [{Duration}], Data = [{data}]";
                }
            }

            public class PrevBasedCalendar<TPrevData> : ICalendar
            {
                public abstract class HelperBase
                {
                    public void Define(CalendarItems container, Calendars<TPrevData>.CalendarItem todefine)
                    {
                        var begin = todefine.Left.PointValue.Value;
                        var end = todefine.Right.PointValue.Value;

                        var left = begin;

                        while (left.CompareTo(end) < 0)
                        {
                            left = ProcessInterval(container, left, end, todefine.Data);
                        }
                    }

                    protected abstract TTime ProcessInterval(CalendarItems container, TTime left, TTime end, TPrevData data);
                    public abstract TData Include(TData a, TData b);
                    public abstract TData Exclude(TData a, TData b);
                }

                public delegate void DefineData(CalendarItems container, Calendars<TPrevData>.CalendarItem toDefine);

                private readonly Calendars<TPrevData>.ICalendar _prev;
                private readonly DefineData _defineData;
                private readonly CalendarItems _calendarItems;
                //private readonly List<IC> _nexts = new List<IC>();

                public PrevBasedCalendar(DefineData defineData, CalendarItems.IncludeData includeData, CalendarItems.ExcludeData excludeData, CalendarItems.DataToString dataToString = null)
                    : this(null, defineData, includeData, excludeData, dataToString)
                { }

                public PrevBasedCalendar(Calendars<TPrevData>.ICalendar prev, DefineData defineData, CalendarItems.IncludeData includeData, CalendarItems.ExcludeData excludeData, CalendarItems.DataToString dataToString = null)
                {
                    // проверить на закольцовывание... на вс€кий случай....
                    if (prev?.FullChain().Contains(this) == true)
                        throw new ArgumentException("try to loop detected", nameof(prev));

                    _prev = prev;
                    //_prev?._nexts.Add(this);

                    _defineData = defineData;
                    _calendarItems = new CalendarItems(includeData, excludeData, dataToString);
                }

                public PrevBasedCalendar(HelperBase helper, CalendarItems.DataToString dataToString = null)
                : this(helper.Define, helper.Include, helper.Exclude, dataToString)
                {
                }

                public PrevBasedCalendar(Calendar<TPrevData> prev, HelperBase helper, CalendarItems.DataToString dataToString = null)
                : this(prev, helper.Define, helper.Include, helper.Exclude, dataToString)
                {
                }

                public IAbstractCalendar[] FullChain()
                {
                    var chain = new IAbstractCalendar[] { this };

                    if (_prev != null)
                        chain = _prev.FullChain().Union(chain).ToArray();

                    return chain;
                }

                public IEnumerable<CalendarItem> Get(TTime from, TTime to)
                {

                    var result = new List<CalendarItem>();

                    var left = Point<TTime>.Left(from, false);
                    var right = Point<TTime>.Right(to, false);

                    var node = _calendarItems.FindNode(x => x.Contains(left));

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
                    if (undefinedInterval == null)
                        throw new ArgumentNullException(nameof(undefinedInterval));

                    var intervals =
                        _prev?.Get(undefinedInterval.Left.PointValue.Value, undefinedInterval.Right.PointValue.Value)
                        ?? new[] { new Calendars<TPrevData>.CalendarItem(undefinedInterval.Left, undefinedInterval.Right) };



                    foreach (var interval in intervals)
                    {
                        _defineData(_calendarItems, interval);
                    }
                }

                public override string ToString()
                {
                    return $"{GetType().TypeName()}";
                }
            }
        }

        public Calendar<TData> CreateCalendar<TData>(Calendars<TData>.PrevBasedCalendar<TData>.DefineData defineData, Calendars<TData>.CalendarItems.IncludeData includeData, Calendars<TData>.CalendarItems.ExcludeData excludeData, Calendars<TData>.CalendarItems.DataToString dataToString = null)
        {
            return new Calendar<TData>(defineData, includeData, excludeData, dataToString);
        }

        public class Calendar<TData> : Calendars<TData>.PrevBasedCalendar<TData>
        {
            public Calendar(DefineData defineData, Calendars<TData>.CalendarItems.IncludeData includeData, Calendars<TData>.CalendarItems.ExcludeData excludeData, Calendars<TData>.CalendarItems.DataToString dataToString = null)
                : base(defineData, includeData, excludeData, dataToString)
            {
            }

            public Calendar(Calendar<TData> prev, DefineData defineData, Calendars<TData>.CalendarItems.IncludeData includeData, Calendars<TData>.CalendarItems.ExcludeData excludeData, Calendars<TData>.CalendarItems.DataToString dataToString = null)
                : base(prev, defineData, includeData, excludeData, dataToString)
            {
            }

            public Calendar(Calendars<TData>.HelperBase helper, Calendars<TData>.CalendarItems.DataToString dataToString = null)
                : this(helper.Define, helper.Include, helper.Exclude, dataToString)
            {
            }

            public Calendar(Calendar<TData> prev, Calendars<TData>.HelperBase helper, Calendars<TData>.CalendarItems.DataToString dataToString = null)
                : this(prev, helper.Define, helper.Include, helper.Exclude, dataToString)
            {
            }
        }

    }

}