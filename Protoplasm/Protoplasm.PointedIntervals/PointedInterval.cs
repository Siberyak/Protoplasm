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
    public class PointedInterval<TBound, TData> : Interval<TBound>
        where TBound : struct, IComparable<TBound>
    {

        /// <summary>
        /// Возвращает строку, представляющую текущий объект.
        /// </summary>
        /// <returns>
        /// Строка, представляющая текущий объект.
        /// </returns>
        public override string ToString()
        {
            var data = DataToString == null ? (object)Data : DataToString(Data);
            return $"{base.ToString()}, Data = [{data}]";
        }

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
        protected internal PointedInterval(TBound? left, bool leftIncluded, TBound? right, bool rightIncluded, TData data)
            : this(left.HasValue ? Point<TBound>.Left(left.Value, leftIncluded) : Point<TBound>.Left(), right.HasValue ? Point<TBound>.Right(right.Value, rightIncluded) : Point<TBound>.Right(), data)
        { }

        protected internal PointedInterval(Point<TBound> left = null, Point<TBound> right = null, TData data = default(TData))
            : base(left, right)
        {

            Data = data;
        }


        protected internal bool TrySplit(Point<TBound> point, out Point<TBound>[] points, bool selfAsSplitted = false)
        {
            points = null;

            if (!Contains(point))
                return false;

            // [[, ]], ((, )) 
            if (Left.Equals(point) || Right.Equals(point))
            {
                if(selfAsSplitted)
                {
                    points = new[] {Left, Right};
                    return true;
                }

                return false;
            }


            var equalsToLeft = Equals(Left.PointValue, point.PointValue);
            var equalsToRight = Equals(Right.PointValue, point.PointValue);

            // не попадаем в грвницу
            if (!equalsToLeft && !equalsToRight)
            {
                var pair = point.Split();
                points = new[] {Left, pair[0], pair[1], Right};
            }
            // левая граница ([, [(
            // = [](*
            else if (equalsToLeft)
            {
                points = new[] {Point<TBound>.Left(point), Point<TBound>.Right(point), Point<TBound>.Left(point, false), Right};

            }
            // правая граница )], ])
            // = *)[]
            else
            {
                points = new[] { Left, Point<TBound>.Right(point, false), Point<TBound>.Left(point), Point<TBound>.Right(point) } ;
            }

            return true;
        }


        
    }
}