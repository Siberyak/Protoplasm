using System;

namespace Protoplasm.PointedIntervals
{
    public class Interval<TBound> where TBound : struct, IComparable<TBound>
    {
        public static readonly Interval<TBound> Undefined = new Interval<TBound>(Point<TBound>.Left(), Point<TBound>.Right());
        public readonly Point<TBound> Left;
        public readonly Point<TBound> Right;

        /// <summary>
        /// Обе границы не определены
        /// </summary>
        public bool IsUndefined => Left.IsUndefined && Right.IsUndefined;
        /// <summary>
        /// Обе границы определены
        /// </summary>
        public bool IsDefined => !Left.IsUndefined && !Right.IsUndefined;
        /// <summary>
        /// Определена только одна из границ
        /// </summary>
        public bool IsPartialDefined => !IsDefined && !IsUndefined;

        public Interval(TBound? left, TBound? right, bool leftIncluded, bool rightIncluded)
            : this(Point<TBound>.Left(left, left.HasValue && leftIncluded), Point<TBound>.Right(right, right.HasValue && rightIncluded))
        {
        }

        public Interval(Point<TBound> left = null, Point<TBound> right = null)
        {
            Left = left ?? Point<TBound>.Left();
            Right = right ?? Point<TBound>.Right();

            if (Left.Direction != PointDirection.Left)
                throw new ArgumentException("Direction != PointDirection.Left", nameof(left));

            if (Right.Direction != PointDirection.Right)
                throw new ArgumentException("Direction != PointDirection.Right", nameof(right));

            if (Left > Right)
                throw new Exception("left > right");
        }

        public static Interval<TBound> New(TBound value)
        {
            return new Interval<TBound>(value, value, true, true);
        }
        public static Interval<TBound> New(TBound begin, TBound end, bool leftIncluded = true, bool rightIncluded = false)
        {
            return new Interval<TBound>(begin, end, leftIncluded, rightIncluded);
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

        public override string ToString()
        {
            return $"{Left}, {Right}";
        }
    }
}