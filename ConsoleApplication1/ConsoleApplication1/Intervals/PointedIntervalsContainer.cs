#define CHECK1


using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1.Intervals
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBound"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public class PointedIntervalsContainer<TBound, TData>
        where TBound : struct, IComparable<TBound>
    {

        static PointedIntervalsContainer()
        {
            PointedIntervalsContainerExtender.Test();
        }

        private readonly IncludeData _includeData;

        private readonly ExcludeData _excludeData;

        private readonly DataToString _dataToString;

        private LinkedList<PointedInterval<TBound, TData>> _is;

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="T:System.Object"/>.
        /// </summary>
        public PointedIntervalsContainer(IncludeData includeData, ExcludeData excludeData, DataToString dataToString = null)
        {
            _includeData = includeData;
            _excludeData = excludeData;
            _dataToString = dataToString;
            _is = new LinkedList<PointedInterval<TBound, TData>>(new[] { new PointedInterval<TBound, TData>() { DataToString = _dataToString } });
        }

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

        private PointedIntervalsContainer<TBound, TData> Process(PointedInterval<TBound, TData> interval, Func<TData, TData, TData> func)
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
                    var nodeData = new PointedInterval<TBound, TData>(previous.Left, current.Right, current.Data) {DataToString = current.DataToString};

                    node = prev == null
                        ? _is.AddFirst(nodeData)
                        : _is.AddAfter(prev, nodeData);
                }

                node = node.Next;

                //if (current.Left > interval.Right)
                //    break;
            }

            return this;
        }

        private LinkedListNode<PointedInterval<TBound, TData>> UnionWithPrevious(LinkedListNode<PointedInterval<TBound, TData>> node)
        {
            var nextNode = node?.Next;
            if (nextNode == null)
                return null;

            var data = node.Value.Data;

            if (!Equals(data, nextNode.Value.Data))
                return nextNode;

            var a = node.Value;
            var b = nextNode.Value;

            var previous = node.Previous;

            _is.Remove(node);
            _is.Remove(nextNode);

            var interval = new PointedInterval<TBound, TData>(a.Left, b.Right, data);

            return previous == null
                ? _is.AddFirst(interval)
                : _is.AddAfter(previous, interval);
        }

        private LinkedListNode<PointedInterval<TBound, TData>> UnionWithNext(LinkedListNode<PointedInterval<TBound, TData>> node)
        {
            var nextNode = node?.Next;
            if (nextNode == null)
                return null;

            var data = node.Value.Data;

            if (!Equals(data, nextNode.Value.Data))
                return nextNode;

            var a = node.Value;
            var b = nextNode.Value;

            var previous = node.Previous;

            _is.Remove(node);
            _is.Remove(nextNode);

            var interval = new PointedInterval<TBound, TData>(a.Left, b.Right, data);

            return previous == null
                ? _is.AddFirst(interval)
                : _is.AddAfter(previous, interval);
        }

        public PointedInterval<TBound, TData> Left => _is.Count == 0 ? null : _is.First.Next?.Value;
        public PointedInterval<TBound, TData> Right => _is.Count == 0 ? null : _is.Last.Previous?.Value;

        private IEnumerable<LinkedListNode<PointedInterval<TBound, TData>>> GetNodes(Point<TBound> left, Point<TBound> right)
        {
            var intervals = _is.ToArray();
            var index = BinaryFind(intervals, left, 0, intervals.Length);
            var item = intervals[index];

            if (item.Left != left)
            {
                if (index > 0 && intervals[index - 1].Left == left)
                {
                    item = intervals[index - 1];
                    index--;
                }
                else if(index < intervals.Length - 1 && intervals[index + 1].Left == left)
                {
                    item = intervals[index + 1];
                    index++;
                }
                else
                    throw new Exception();
            }

#if CHECK
            var item2 = intervals.SkipWhile(x => x.Left != left).First();
            if(item != item2)
                throw new Exception();
#endif

            

            LinkedListNode<PointedInterval<TBound, TData>> node = GetNodeByIndex(index);
#if CHECK
            var node1 = _is.Find(item);
            if(node != node1)
                throw new Exception();
#endif

            while (node != null)
            {
                item = node.Value;
                yield return node;

                if (item.Right == right)
                    yield break;

                node = node.Next;
            }
        }

        private LinkedListNode<PointedInterval<TBound, TData>> GetNodeByIndex(int index)
        {
            if (_is.Count / 2 >= index)
            {
                var node = _is.First;
                for (int i = 0; i < index; i++)
                {
                    node = node.Next;
                }

                return node;
            }
            else
            {
                var node = _is.Last;
                for (int i = _is.Count - 1; i > index; i--)
                {
                    node = node.Previous;
                }

                return node;
            }
        }

        private LinkedListNode<PointedInterval<TBound, TData>>[] Add(Point<TBound> point)
        {
            //Point<TBound> point = isLeft ? Point<TBound>.Left(value, included) : Point<TBound>.Right(value, included);

            var intervals = _is.ToArray();
            int index = BinaryFind(intervals, point, 0, intervals.Length);
            var interval = intervals[index];


#if CHECK
            var n1 = _is.Skip(index).First(x => x.Contains(point));
            if (n1 != interval)
                throw new Exception();
#endif



#if CHECK
            var i0 = Find2(point);

            if(i0 != interval)
                throw new Exception();

#endif

            var node = GetNodeByIndex(index);

#if CHECK
            var node1 = _is.Find(interval);
            if (node != node1)
                throw new Exception();
#endif


            var prev = node.Previous;


            var items = interval.Split(point).ToArray();

            var nodes = new LinkedListNode<PointedInterval<TBound, TData>>[] {prev, null};

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

        private PointedInterval<TBound, TData> Find2(Point<TBound> point)
        {
            return _is.First(x => x.Contains(point));
        }

        //        private PointedInterval<TBound, TData> FindIntervalByPoint(Point<TBound> point)
        //        {
        //            int count = _is.Count;
        //            if(count < 2)
        //                return _is.First(x => x.Contains(point));

        //            var intervals = _is.ToArray();
        //            int index = BinaryFind(intervals, point, 0, count);
        //            var n2 = intervals[index];
        //#if CHECK
        //            var n1 = _is.Skip(index).First(x => x.Contains(point));
        //            if (n1 != n2)
        //                throw new Exception();
        //#endif

        //            return n2;
        //        }

        private
#if !CHECK
static 
#endif
            int BinaryFind(IReadOnlyList<PointedInterval<TBound, TData>> intervals, Point<TBound> point, int startIndex, int length)
        {
            int lengthHalf = length / 2;
            int middleIndex = startIndex + lengthHalf;
            var middle = intervals[middleIndex];
#if CHECK
            var middle2 = _is.Skip(middleIndex).First();
            if(middle2 != middle)
                throw new Exception();
#endif


            if (middle.Right < point)
            {
                return BinaryFind(intervals, point, middleIndex, length - lengthHalf);
            }

            else if (middle.Left > point)
            {
                return BinaryFind(intervals, point, startIndex, lengthHalf);
            }

            return middleIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public PointedIntervalsContainer<TBound, TData> Include(PointedInterval<TBound, TData> i)
        {
            return Process(i, (x, y) => _includeData(x, y));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public PointedIntervalsContainer<TBound, TData> Exclude(PointedInterval<TBound, TData> i)
        {
            return Process(i, (x, y) => _excludeData(x, y));
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
        public PointedIntervalsContainer<TBound, TData> Include(TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
        {
            return Include(new PointedInterval<TBound, TData>(left, leftIncluded, right, rightIncluded, data));
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
        public PointedIntervalsContainer<TBound, TData> Exclude(TBound? left, TBound? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
        {
            return Exclude(new PointedInterval<TBound, TData>(left, leftIncluded, right, rightIncluded, data));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PointedInterval<TBound, TData>[] ToArray(bool onlyWithData = true)
        {
            PointedInterval<TBound, TData>[] array = _is.ToArray();
            if (onlyWithData)
                array = array.Where(x => !Equals(x.Data, default(TData))).ToArray();

                //array = array.Length > 1 
                //    ? array.Skip(1).Take(array.Length-2).ToArray()
                //    : new PointedInterval<TBound, TData>[0];

            return array;
        }
    }
}