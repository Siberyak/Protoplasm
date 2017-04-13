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
    public class PointedInterval<TBound, TData> where TBound : struct, IComparable<TBound>
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
            return $"{Left},{Right} {data}";
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
        protected internal PointedIntervalsContainer<TBound, TData>.DataToString DataToString;

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
        public PointedInterval(TBound? left, bool leftIncluded, TBound? right, bool rightIncluded, TData data)
            : this(left.HasValue ? Point<TBound>.Left(left.Value, leftIncluded) : Point<TBound>.Left(), right.HasValue ? Point<TBound>.Right(right.Value, rightIncluded) : Point<TBound>.Right(), data)
        { }

        internal PointedInterval(Point<TBound> left = null, Point<TBound> right = null, TData data = default(TData))
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

        public IEnumerable<PointedInterval<TBound, TData>> Split(Point<TBound> point)
        {
            // [[, ]], ((, )) 
            if (Left.Equals(point) || Right.Equals(point))
                return new[] {this};

            Point<TBound>[] points;

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

            return new[]
            {
                new PointedInterval<TBound, TData>(points[0], points[1], Data) {DataToString = DataToString},
                new PointedInterval<TBound, TData>(points[2], points[3], Data) {DataToString = DataToString},
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="isLeft"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<PointedInterval<TBound, TData>> Split1(Point<TBound> point, bool isLeft)
        {
            if (point == Left)
            {
                if (point.Direction == PointDirection.Left)
                {
                    if(point.Included)
                    {
                        return new[]
                        {
                            new PointedInterval<TBound, TData>(Point<TBound>.Left(point.PointValue), Point<TBound>.Right(point.PointValue), Data) {DataToString = DataToString},
                            new PointedInterval<TBound, TData>(Point<TBound>.Left(Left.PointValue, false), Right, Data) {DataToString = DataToString},

                        };
                    }

                    return new[]
                    {
                        new PointedInterval<TBound, TData>(Point<TBound>.Left(point), Point<TBound>.Right(point), Data) {DataToString = DataToString},
                        new PointedInterval<TBound, TData>(Point<TBound>.Left(Left, false), Right, Data) {DataToString = DataToString},

                    };
                }
                return new[] { this };
            }

            if (point == Right)
            {
                if (point.Direction == PointDirection.Right)
                {
                    if (point.Included)
                    {
                        return new[]
                        {
                            new PointedInterval<TBound, TData>(Left, Point<TBound>.Right(Right, false), Data) {DataToString = DataToString},
                            new PointedInterval<TBound, TData>(Point<TBound>.Left(point.PointValue), Point<TBound>.Right(point.PointValue), Data) {DataToString = DataToString},
                        };
                    }

                    return new[]
                        {
                            new PointedInterval<TBound, TData>(Left, Point<TBound>.Right(Right, false), Data) {DataToString = DataToString},
                            new PointedInterval<TBound, TData>(Point<TBound>.Left(point), Point<TBound>.Right(point), Data) {DataToString = DataToString},
                        };

                }
                return new[] { this };

            }

            var included = point.Included;

            var rightIncluded = (isLeft && !included) || (!isLeft && included);

            return new[]
                       {
                           new PointedInterval<TBound, TData>(Left, Point<TBound>.Right(point.PointValue, rightIncluded), Data) {DataToString = DataToString},
                           new PointedInterval<TBound, TData>(Point<TBound>.Left(point.PointValue, !rightIncluded), Right , Data){DataToString = DataToString},
                       };
        }

    }
}