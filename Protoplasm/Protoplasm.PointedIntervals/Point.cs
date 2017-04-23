using System;
using System.Collections.Generic;

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
        /// ���������, ����� �� ������� ������ ������� ������� ���� �� ����.
        /// </summary>
        /// <returns>
        /// true, ���� ������� ������ ����� ��������� <paramref name="other"/>, � ��������� �����堗 false.
        /// </returns>
        /// <param name="other">������, ������� ��������� �������� � ������ ��������.</param>
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
        /// ����������, ����� �� �������� ������ �������� �������.
        /// </summary>
        /// <returns>
        /// true, ���� ��������� ������ ����� �������� �������; � ��������� �����堗 false.
        /// </returns>
        /// <param name="obj">������, ������� ��������� �������� � ������� ��������.</param>
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
        /// ������ ���� ���-������� ��� ������������� ����.
        /// </summary>
        /// <returns>
        /// ���-��� ��� �������� �������.
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
        /// �������������� ����� ��������� ������ <see cref="T:System.Object"/>.
        /// </summary>
        private Point(TBound? pointValue, PointDirection direction, bool included)
        {
            PointValue = pointValue;
            Direction = direction;
            Included = !Equals(pointValue, default(TBound?)) && included;
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

        /// <summary>
        /// <paramref name="a"/> ������ <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) > 0;
        }

        /// <summary>
        /// <paramref name="a"/> ����� <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) < 0;
        }


        /// <summary>
        /// <paramref name="a"/> ������ ��� ��������� � <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >=(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) >= 0;
        }

        /// <summary>
        /// <paramref name="a"/> ����� ��� ��������� � <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <=(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) <= 0;
        }

        /// <summary>
        /// <paramref name="a"/> ��������� � <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Point<TBound> a, Point<TBound> b)
        {
            return _comparer.Compare(a, b) == 0;
        }

        /// <summary>
        /// <paramref name="a"/> �� ��������� � <paramref name="b"/>
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
            /// ��������� ���� �������� � ������� ��������, ������������, �������� �� ���� ������ �������, ������ ��� ������� �������.
            /// </summary>
            /// <returns>
            /// �������� ����� �����, ������� ���������� ������������� �������� ���������� <paramref name="a"/> � <paramref name="b"/>, ��� �������� � ��������� �������. ��������  ��������  ������ ���� �������� ��������� <paramref name="a"/> ������ �������� ��������� <paramref name="b"/>. Zero �������� ���������� <paramref name="a"/> � <paramref name="b"/> �����. ������ ����. �������� <paramref name="a"/> ������ �������� <paramref name="b"/>.
            /// </returns>
            /// <param name="a">������ ������������ ������.</param><param name="b">������ ������������ ������.</param>
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
        /// ���������� ������, �������������� ������� ������.
        /// </summary>
        /// <returns>
        /// ������, �������������� ������� ������.
        /// </returns>
        public override string ToString()
        {
            string format = Direction == PointDirection.Left ? "{0}{1}" : "{1}{0}";
            return string.Format(format, PointsComparerBase.Brackets[Direction][Included], PointValue);
        }

        public Point<TBound>[] Split()
        {
            return Direction == PointDirection.Left 
                ? new[] {Right(this, !Included), this} 
                : new[] { this, Left(this, !Included) };
        }
    }
}