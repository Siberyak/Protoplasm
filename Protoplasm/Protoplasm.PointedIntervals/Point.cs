using System;
using System.Collections.Generic;
using System.Linq;

namespace Protoplasm.PointedIntervals
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBound"></typeparam>
    public class Point<TBound> : IEquatable<Point<TBound>>
        where TBound : struct, IComparable<TBound>
    {
        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public bool Equals(Point<TBound> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return PointValue.Equals(other.PointValue) && Direction == other.Direction && Included == other.Included;
        }

        /// <summary>
        /// Определяет, равен ли заданный объект текущему объекту.
        /// </summary>
        /// <returns>
        /// true, если указанный объект равен текущему объекту; в противном случае — false.
        /// </returns>
        /// <param name="obj">Объект, который требуется сравнить с текущим объектом.</param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Point<TBound>)obj);
        }

        /// <summary>
        /// Играет роль хэш-функции для определенного типа.
        /// </summary>
        /// <returns>
        /// Хэш-код для текущего объекта.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PointValue.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Direction;
                hashCode = (hashCode * 397) ^ Included.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly TBound? PointValue;

        /// <summary>
        /// 
        /// </summary>
        public readonly PointDirection Direction;

        /// <summary>
        /// 
        /// </summary>
        public readonly bool Included;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.Object"/>.
        /// </summary>
        private Point(TBound? pointValue, PointDirection direction, bool included)
        {
            PointValue = pointValue;
            Direction = direction;
            Included = pointValue.HasValue && included;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="included"></param>
        /// <returns></returns>
        public static Point<TBound> Left(TBound? value, bool included = true)
        {
            return new Point<TBound>(value, PointDirection.Left, included);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Point<TBound> Left()
        {
            return new Point<TBound>(default(TBound?), PointDirection.Left, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="included"></param>
        /// <returns></returns>
        public static Point<TBound> Right(TBound? value, bool included = true)
        {
            return new Point<TBound>(value, PointDirection.Right, included);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Point<TBound> Right()
        {
            return new Point<TBound>(default(TBound?), PointDirection.Right, false);
        }

        public Point<TBound> AsRight(bool? included = null)
        {
            return Direction == PointDirection.Right && included == Included
                ? this
                : Right(PointValue, included ?? !Included);
        }

        public Point<TBound> AsLeft(bool? included = null)
        {
            return Direction == PointDirection.Left && included == Included
                ? this
                : Left(PointValue, included ?? !Included);
        }

        /// <summary>
        /// <paramref name="a"/> правее <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) > 0;
        }

        /// <summary>
        /// <paramref name="a"/> левее <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) < 0;
        }


        /// <summary>
        /// <paramref name="a"/> правее или совпадает с <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >=(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) >= 0;
        }

        /// <summary>
        /// <paramref name="a"/> левее или совпадает с <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <=(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) <= 0;
        }

        /// <summary>
        /// <paramref name="a"/> совпадает с <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) == 0;
        }

        /// <summary>
        /// <paramref name="a"/> не совпадает с <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) != 0;
        }

        public static implicit operator TBound?(Point<TBound> point)
        {
            return point.PointValue;
        }


        static readonly PointsComparer _comparer = new PointsComparer();

        class PointsComparer : PointsComparerBase, IComparer<Point<TBound>>
        {
            /// <summary>
            /// Сравнение двух объектов и возврат значения, указывающего, является ли один объект меньшим, равным или большим другого.
            /// </summary>
            /// <returns>
            /// Знаковое целое число, которое определяет относительные значения параметров <paramref name="a"/> и <paramref name="b"/>, как показано в следующей таблице. Значение  Значение  Меньше нуля Значение параметра <paramref name="a"/> меньше значения параметра <paramref name="b"/>. Zero Значения параметров <paramref name="a"/> и <paramref name="b"/> равны. Больше нуля. Значение <paramref name="a"/> больше значения <paramref name="b"/>.
            /// </returns>
            /// <param name="a">Первый сравниваемый объект.</param><param name="b">Второй сравниваемый объект.</param>
            public int Compare(Point<TBound> a, Point<TBound> b)
            {
                TBound? aa = a.PointValue;
                TBound? bb = b.PointValue;

                if (!aa.HasValue && !bb.HasValue)
                    return a.Direction == b.Direction ? 0 : ByDirection(a.Direction);

                if (!aa.HasValue)
                {
                    return ByDirection(a.Direction);
                }

                if (!bb.HasValue)
                {
                    return -1 * ByDirection(b.Direction);
                }

                int byValue = aa.Value.CompareTo(bb.Value);
                if (byValue != 0)
                {
                    return byValue;
                }

                //string str = Brackets[a.Direction][a.Included] + Brackets[b.Direction][b.Included];
                //return OnEqualsValues[str];

                string str = $"{GetBrackets(a)}{GetBrackets(b)}";
                return IsEqualsValues(str);

            }

            private int IsEqualsValues(string brackets)
            {
                switch (brackets)
                {
                    case "((": return 0; // ( == (
                    case "()": return 1; // ( > )
                    case "([": return 1; // ( > [
                    case "(]": return 1; // ( > ]

                    case "))": return 0; // ) == )
                    case ")(": return -1; // ) < (
                    case ")[": return -1; // ) < [
                    case ")]": return -1; // ) < ]

                    case "[[": return 0; // [ == [
                    case "[]": return 0; // [ == ]
                    case "[(": return -1; // [ < (
                    case "[)": return 1; // [ > )

                    case "]]": return 0; // ] == ]
                    case "][": return 0; // ] == [
                    case "](": return -1; // ] < (
                    case "])": return 1; // ] > )

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private static string GetBrackets(Point<TBound> point)
            {
                switch (point.Direction)
                {
                    case PointDirection.Left:
                        return point.Included ? "[" : "(";
                    case PointDirection.Right:
                        return point.Included ? "]" : ")";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            static int ByDirection(PointDirection direction)
            {
                return direction == PointDirection.Left ? -1 : 1;
            }
        }

        static Point()
        {
            PointsComparerBase.SelfTest<TBound>();
        }

        /// <summary>
        /// Возвращает строку, представляющую текущий объект.
        /// </summary>
        /// <returns>
        /// Строка, представляющая текущий объект.
        /// </returns>
        public override string ToString()
        {
            string format = Direction == PointDirection.Left ? "{0}{1}" : "{1}{0}";
            return string.Format(format, PointsComparerBase.Brackets[Direction][Included], PointValue);
        }

        public Point<TBound>[] Split()
        {
            return Direction == PointDirection.Left 
                ? new[] { AsRight(), this} 
                : new[] { this, AsLeft() };
        }

        public static Point<TBound> Min(Point<TBound> a, Point<TBound> b, params Point<TBound>[] other)
        {
            var result = a <= b ? a : b;
            return other.Aggregate(result, (current, point) => Min(current, point));
        }

        public static Point<TBound> Max(Point<TBound> a, Point<TBound> b, params Point<TBound>[] other)
        {
            var result = a >= b ? a : b;
            return other.Aggregate(result, (current, point) => Max(current, point));
        }

        public bool IsUndefined => !PointValue.HasValue;
    }
}