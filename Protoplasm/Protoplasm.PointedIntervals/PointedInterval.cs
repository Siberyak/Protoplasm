using System;
using System.Collections.Generic;
using System.Linq;

namespace Protoplasm.PointedIntervals
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBound"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public abstract class PointedInterval<TBound, TData> where TBound : struct, IComparable<TBound>
    {
        /// <summary>
        /// ���������� ������, �������������� ������� ������.
        /// </summary>
        /// <returns>
        /// ������, �������������� ������� ������.
        /// </returns>
        public override string ToString()
        {
            var data = DataToString == null ? (object)Data : DataToString(Data);
            return $"{Left}, {Right}, Data = [{data}]";
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly Point<TBound> Left;
        /// <summary>
        /// 
        /// </summary>
        public readonly Point<TBound> Right;

        /// <summary>
        /// 
        /// </summary>
        protected internal PointedIntervalsContainerBase<TBound, TData>.DataToString DataToString;

        /// <summary>
        /// 
        /// </summary>
        public TData Data { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="leftIncluded"></param>
        /// <param name="right"></param>
        /// <param name="rightIncluded"></param>
        /// <param name="data"></param>
        protected PointedInterval(TBound? left, bool leftIncluded, TBound? right, bool rightIncluded, TData data)
            : this(left.HasValue ? Point<TBound>.Left(left.Value, leftIncluded) : Point<TBound>.Left(), right.HasValue ? Point<TBound>.Right(right.Value, rightIncluded) : Point<TBound>.Right(), data)
        { }

        protected PointedInterval(Point<TBound> left = null, Point<TBound> right = null, TData data = default(TData))
        {
            Left = left ?? Point<TBound>.Left();
            Right = right ?? Point<TBound>.Right();

            if (Left > Right)
                throw new Exception("left > right");

            Data = data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Point<TBound> point)
        {
            return Left <= point && point <= Right;
        }

        internal bool TrySplit(Point<TBound> point, out Point<TBound>[] points)
        {
            points = null;

            // [[, ]], ((, )) 
            if (Left.Equals(point) || Right.Equals(point))
                return false;


            var equalsToLeft = Equals(Left.PointValue, point.PointValue);
            var equalsToRight = Equals(Right.PointValue, point.PointValue);

            // �� �������� � �������
            if (!equalsToLeft && !equalsToRight)
            {
                var pair = point.Split();
                points = new[] {Left, pair[0], pair[1], Right};
            }
            // ����� ������� ([, [(
            // = [](*
            else if (equalsToLeft)
            {
                points = new[] {Point<TBound>.Left(point), Point<TBound>.Right(point), Point<TBound>.Left(point, false), Right};

            }
            // ������ ������� )], ])
            // = *)[]
            else
            {
                points = new[] { Left, Point<TBound>.Right(point, false), Point<TBound>.Left(point), Point<TBound>.Right(point) } ;
            }

            return true;
        }


        
    }
}