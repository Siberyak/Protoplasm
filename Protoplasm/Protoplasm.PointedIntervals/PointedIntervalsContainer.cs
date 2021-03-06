using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Protoplasm.PointedIntervals
{
    public interface IIntervalData<T>
    {
        /// <summary>
        /// ���������� ����� ������� (!!!) �������� <typeparamref name="T"/>>, ������� �������� ����������� ����������� "��������" [Include] ��� �������� �������� � <paramref name="b"/>.
        /// ��������, ���� ��������� <typeparamref name="T"/> �������� <see cref="int"/>, �� ��� ��������: a.Include(b) ������������ a + b
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        T Include(T b);

        /// <summary>
        /// ���������� [����� �������] (!!!) �������� <typeparamref name="T"/>, ������� �������� ����������� ����������� "��������" [Exclude] ��� �������� �������� � <paramref name="b"/>.
        /// ��������, ���� ��������� <typeparamref name="T"/> �������� <see cref="int"/>, �� ��� ���������: a.Exclude(b) ������������ a - b
        ///  </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        T Exclude(T b);
    }


    public abstract class PointedIntervalsContainerBase<TBound, TData>
        where TBound : struct, IComparable<TBound>
    {
        /// <summary>
        /// a '+' b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public delegate TData IncludeData(TData a, TData b);

        /// <summary>
        /// a '-' b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public delegate TData ExcludeData(TData a, TData b);

        /// <summary>
        /// data -> 'ToString()'
        /// </summary>
        /// <param name="data"></param>
        public delegate string DataToString(TData data);


        public abstract PointedIntervalsContainerBase<TBound, TData> Exclude(PointedInterval<TBound, TData> interval);
        public abstract PointedIntervalsContainerBase<TBound, TData> Exclude(Point<TBound> left, Point<TBound> right, TData data);
        public abstract PointedIntervalsContainerBase<TBound, TData> Exclude(TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true);
        public abstract PointedIntervalsContainerBase<TBound, TData> Include(PointedInterval<TBound, TData> interval);
        public abstract PointedIntervalsContainerBase<TBound, TData> Include(Point<TBound> left, Point<TBound> right, TData data);
        public abstract PointedIntervalsContainerBase<TBound, TData> Include(TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true);

        public abstract PointedInterval<TBound, TData>[] DefinedItems(bool onlyWithData = true);


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

        public TData Include(TData a, TData b)
        {
            return _includeData(a, b);
        }
        public TData Exclude(TData a, TData b)
        {
            return _excludeData(a, b);
        }

        private readonly CreateInterval _createInterval;
        private readonly IncludeData _includeData;

        private readonly ExcludeData _excludeData;

        private readonly DataToString _dataToString;

        //private LinkedList<TInterval> _is;
        private readonly SimpleLinkedList<TInterval> _intervals;

        public INode<TInterval> FirstNode => _intervals.First;
        public INode<TInterval> LastNode => _intervals.Last;

        /// <summary>
        /// �������������� ����� ��������� ������ <see cref="T:System.Object"/>.
        /// </summary>
        public PointedIntervalsContainer(CreateInterval createInterval, IncludeData includeData, ExcludeData excludeData, DataToString dataToString = null)
        {
            _createInterval = createInterval;
            _includeData = includeData;
            _excludeData = excludeData;
            _dataToString = dataToString;

            //_is = new LinkedList<TInterval>(new[] { NewInterval() });
            _intervals = new SimpleLinkedList<TInterval>(new[] { NewInterval() });
        }

        public TInterval NewInterval(Point<TBound> left = null, Point<TBound> right = null, TData data = default(TData), DataToString dataToString = null)
        {
            var interval = _createInterval(left, right, data);
            interval.DataToString = dataToString ?? _dataToString;
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


            while (node != null && node.Previous != right[1])
            {
                var current = node.Value;
                if (interval.Left <= current.Right && current.Left <= interval.Right)
                {
                    var data = func(current.Data, interval.Data);
                    current.Data = data;
                }

                node = TryMerge(node);

                node = node.Next;
            }

            TryMerge(node);

            return this;
        }

        private INode<TInterval> TryMerge(INode<TInterval> node)
        {
            if (node == null)
                return null;

            var current = node.Value;
            var previous = node.Previous?.Value;
            if (previous != null && Equals(previous.Data, current.Data))
            {
                var prev = node.Previous.Previous;
                _intervals.Remove(node.Previous);
                _intervals.Remove(node);
                var nodeData = NewInterval(previous.Left, current.Right, current.Data, current.DataToString);

                node = prev == null
                    ? _intervals.AddFirst(nodeData)
                    : _intervals.AddAfter(prev, nodeData);
            }
            return node;
        }

        public TInterval Left => _intervals.Count == 0 ? null : _intervals.First.Next?.Value;
        public TInterval Right => _intervals.Count == 0 ? null : _intervals.Last.Previous?.Value;

        private INode<TInterval>[] Add(Point<TBound> point)
        {

            var node = _intervals.Find(x => x.Contains(point));

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
                    : new[] { interval };


            var nodes = new[] { prev, null };

            //if (items.Length > 1)
            {
                _intervals.Remove(node);

                foreach (var item in items)
                {
                    prev = prev == null
                        ? _intervals.AddFirst(item)
                        : _intervals.AddAfter(prev, item);

                    nodes[0] = nodes[0] ?? prev;
                    nodes[1] = prev;
                }
            }

            return nodes;
        }


        public INode<TInterval> FindNode(Func<TInterval, bool> predicate)
        {
            return _intervals.Find(predicate);

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

        //=================
        public override PointedIntervalsContainerBase<TBound, TData> Include(Point<TBound> left, Point<TBound> right, TData data)
        {
            TInterval interval;
            return Include(out interval, left, right, data);
        }

        public PointedIntervalsContainerBase<TBound, TData> Include(out TInterval interval, Point<TBound> left, Point<TBound> right, TData data)
        {
            interval = NewInterval(left, right, data);
            return Include(interval);
        }



        public override PointedIntervalsContainerBase<TBound, TData> Include(TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
        {
            TInterval interval;
            return Include(out interval, left, right, data, leftIncluded, rightIncluded);
        }

        public PointedIntervalsContainerBase<TBound, TData> Include(out TInterval interval, TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
        {
            return Include(out interval, Point<TBound>.Left(left, leftIncluded), Point<TBound>.Right(right, rightIncluded), data);
        }

        //========================

        public PointedIntervalsContainerBase<TBound, TData> Exclude(out TInterval interval, Point<TBound> left, Point<TBound> right, TData data)
        {
            interval = NewInterval(left, right, data);
            return Exclude(interval);
        }

        public override PointedIntervalsContainerBase<TBound, TData> Exclude(Point<TBound> left, Point<TBound> right, TData data)
        {
            TInterval interval;
            return Exclude(out interval, left, right, data);
        }
        public PointedIntervalsContainerBase<TBound, TData> Exclude(out TInterval interval, TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
        {
            return Exclude(out interval, Point<TBound>.Left(left, leftIncluded), Point<TBound>.Right(right, rightIncluded), data);
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
        public TInterval[] DefinedItems()
        {
            TInterval[] array = _intervals.Where(x => !Equals(x.Data, default(TData))).ToArray();
            return array;
        }

        public override PointedInterval<TBound, TData>[] DefinedItems(bool onlyWithData = true)
        {
            return DefinedItems();
        }

        public IEnumerator<TInterval> GetEnumerator()
        {
            return _intervals.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<TInterval> Get(Interval<TBound> interval)
        {
            return interval == null ? new TInterval[0] : Get(interval.Left, interval.Right);
        }

        public IEnumerable<TInterval> Get(Point<TBound> left, Point<TBound> right)
        {
            var result = this.SkipWhile(x => !x.Contains(left)).TakeWhile(x => x.Left < right).ToList();

            var first = result.FirstOrDefault();
            if (first != null && first.Left != left)
            {
                result[0] = NewInterval(left, first.Right, first.Data, _dataToString);
            }

            var last = result.LastOrDefault();
            if (last != null && last.Right != right)
            {
                result[result.Count - 1] = NewInterval(last.Left, right, last.Data, _dataToString); 
            }

            return result;
        }
    }

    public class PointedIntervalsContainer<TBound, TData> : PointedIntervalsContainer<PointedInterval<TBound, TData>, TBound, TData>
        where TBound : struct, IComparable<TBound>
    {
        public PointedIntervalsContainer(IncludeData includeData, ExcludeData excludeData, DataToString dataToString = null) 
            : base(CreatePointedInterval, includeData, excludeData, dataToString)
        {
        }

        private static PointedInterval<TBound, TData> CreatePointedInterval(Point<TBound> left, Point<TBound> right, TData data)
        {
            return new PointedInterval<TBound, TData>(left, right, data);
        }
    }

    //public class IntervalsOfIntervalDataContainer<TInterval, TBound, TData> : PointedIntervalsContainer<TInterval, TBound, TData>
    //where TBound : struct, IComparable<TBound>
    //where TInterval : PointedInterval<TBound, TData>
    //where TData : IIntervalData<TData>
    //{
    //    public IntervalsOfIntervalDataContainer(CreateInterval createInterval, DataToString dataToString = null)
    //        : base(createInterval, (a, b) => a.Include(b), (a, b) => a.Exclude(b), dataToString)
    //    {
    //    }
    //}
}