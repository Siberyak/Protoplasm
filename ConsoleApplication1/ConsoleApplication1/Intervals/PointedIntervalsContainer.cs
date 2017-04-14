using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1.Intervals
{
    public abstract class PointedIntervalsContainerBase<TBound, TData>
        where TBound : struct, IComparable<TBound>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public delegate TData IncludeData(TData a, TData b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public delegate TData ExcludeData(TData a, TData b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public delegate string DataToString(TData data);


        public abstract PointedIntervalsContainerBase<TBound, TData> Exclude(PointedInterval<TBound, TData> interval);
        public abstract PointedIntervalsContainerBase<TBound, TData> Exclude(TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true);
        public abstract PointedIntervalsContainerBase<TBound, TData> Include(PointedInterval<TBound, TData> interval);
        public abstract PointedIntervalsContainerBase<TBound, TData> Include(TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true);

        public abstract PointedInterval<TBound, TData>[] ToArray(bool onlyWithData = true);


    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TInterval"></typeparam>
    /// <typeparam name="TBound"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public class PointedIntervalsContainer<TInterval, TBound, TData> : PointedIntervalsContainerBase<TBound, TData>, IEnumerable<TInterval>
        where TBound : struct, IComparable<TBound>
        where TInterval : PointedInterval<TBound, TData>

    {

        static PointedIntervalsContainer()
        {
            PointedIntervalsContainerExtender.Test();
        }

        private readonly CreateInterval _createInterval;
        private readonly IncludeData _includeData;

        private readonly ExcludeData _excludeData;

        private readonly DataToString _dataToString;

        //private LinkedList<TInterval> _is;
        private readonly SimpleLinkedList<TInterval> _is;

        public SimpleLinkedList<TInterval>.Node FirstNode => _is.First;
        public SimpleLinkedList<TInterval>.Node LastNode => _is.Last;

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="T:System.Object"/>.
        /// </summary>
        public PointedIntervalsContainer(CreateInterval createInterval, IncludeData includeData, ExcludeData excludeData, DataToString dataToString = null)
        {
            _createInterval = createInterval;
            _includeData = includeData;
            _excludeData = excludeData;
            _dataToString = dataToString;

            //_is = new LinkedList<TInterval>(new[] { NewInterval() });
            _is = new SimpleLinkedList<TInterval>(new[] { NewInterval() });
        }

        public TInterval NewInterval(Point<TBound> left = null, Point<TBound> right = null, TData data = default (TData), DataToString dataToString = null)
        {
            var interval = _createInterval(left, right, data);
            interval.DataToString = dataToString  ?? _dataToString;
            return interval;
        }

        
        public delegate TInterval CreateInterval(Point<TBound> left, Point<TBound> right, TData data);

        private PointedIntervalsContainer<TInterval, TBound, TData> Process(PointedInterval<TBound, TData> interval, Func<TData, TData, TData> func)
        {
            if (interval == null || Equals(interval.Data, default(TData)))
            {
                return this;
            }


            var left = Add(interval.Left);
            var node = left[0];
            var right = Add(interval.Right);


            while (node !=  null && node.Previous != right[1])
            {
                var current = node.Value;
                if(interval.Left <= current.Right && current.Left <= interval.Right)
                {
                    var data = func(current.Data, interval.Data);
                    current.Data = data;
                }

                var previous = node.Previous?.Value;
                if (previous != null && Equals(previous.Data, current.Data))
                {
                    var prev = node.Previous.Previous;
                    _is.Remove(node.Previous);
                    _is.Remove(node);
                    var nodeData = NewInterval(previous.Left, current.Right, current.Data, current.DataToString);

                    node = prev == null
                        ? _is.AddFirst(nodeData)
                        : _is.AddAfter(prev, nodeData);
                }

                node = node.Next;
            }

            return this;
        }

        public TInterval Left => _is.Count == 0 ? null : _is.First.Next?.Value;
        public TInterval Right => _is.Count == 0 ? null : _is.Last.Previous?.Value;

        private SimpleLinkedList<TInterval>.Node[] Add(Point<TBound> point)
        {

            var node = _is.Find(x => x.Contains(point));

            var prev = node.Previous;


            Point<TBound>[] points;
            var interval = node.Value;

            var items =
                interval.TrySplit(point, out points)
                    ? new[]
                    {
                        NewInterval(points[0], points[1], interval.Data),
                        NewInterval(points[2], points[3], interval.Data),
                    }
                    : new[] {interval};


            var nodes = new[] {prev, null};

            //if (items.Length > 1)
            {
                _is.Remove(node);

                foreach (var item in items)
                {
                    prev = prev == null 
                        ? _is.AddFirst(item) 
                        : _is.AddAfter(prev, item);

                    nodes[0] = nodes[0] ?? prev;
                    nodes[1] = prev;
                }
            }

            return nodes;
        }


        public SimpleLinkedList<TInterval>.Node FindNode(Func<TInterval, bool> predicate)
        {
            return _is.Find(predicate);

            //_is.FirstOrDefault(x => )
            TInterval interval = null;
//            var node = _is.Find(interval);

            throw new NotImplementedException(); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public override PointedIntervalsContainerBase<TBound, TData> Include(PointedInterval<TBound, TData> interval)
        {
            return Process(interval, (x, y) => _includeData(x, y));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public override PointedIntervalsContainerBase<TBound, TData> Exclude(PointedInterval<TBound, TData> interval)
        {
            return Process(interval, (x, y) => _excludeData(x, y));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="leftIncluded"></param>
        /// <param name="right"></param>
        /// <param name="rightIncluded"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override PointedIntervalsContainerBase<TBound, TData> Include(TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
        {
            TInterval interval;
            return Include(out interval, left, right, data, leftIncluded, rightIncluded);
        }

        public PointedIntervalsContainerBase<TBound, TData> Include(out TInterval interval, TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
        {
            interval = NewInterval(Point<TBound>.Left(left, leftIncluded), Point<TBound>.Right(right, rightIncluded), data);
            return Include(interval);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="left"></param>
        /// <param name="leftIncluded"></param>
        /// <param name="right"></param>
        /// <param name="rightIncluded"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public PointedIntervalsContainerBase<TBound, TData> Exclude(out TInterval interval, TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
        {
            interval = NewInterval(Point<TBound>.Left(left, leftIncluded), Point<TBound>.Right(right, rightIncluded), data);
            return Exclude(interval);
        }

        public override PointedIntervalsContainerBase<TBound, TData> Exclude(TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
        {
            TInterval interval;
            return Exclude(out interval, left, right, data, leftIncluded, rightIncluded);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TInterval[] ToArray()
        {
            TInterval[] array = _is.Where(x => !Equals(x.Data, default(TData))).ToArray();
            return array;
        }

        public override PointedInterval<TBound, TData>[] ToArray(bool onlyWithData = true)
        {
            return ToArray();
        }

        public IEnumerator<TInterval> GetEnumerator()
        {
            return _is.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}