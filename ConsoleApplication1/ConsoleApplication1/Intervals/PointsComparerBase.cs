using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApplication1.Intervals
{
    abstract class PointsComparerBase
    {
        public static readonly Dictionary<PointDirection, Dictionary<bool, string>> Brackets
            = new Dictionary<PointDirection, Dictionary<bool, string>>
                  {
                      {
                          PointDirection.Left, new Dictionary<bool, string>
                                                   {
                                                       { true, "[" },
                                                       { false, "(" }
                                                   }
                      },
                      {
                          PointDirection.Right, new Dictionary<bool, string>
                                                    {
                                                        { true, "]" },
                                                        { false, ")" }
                                                    }
                      },
                  };

        protected static readonly Dictionary<string, int> OnEqualsValues
            = new Dictionary<string, int>
                  {

                      { "((", 0 }, // ( == (
                      { "()", 1 }, // ( > )
                      { "([", 1 }, // ( > [
                      { "(]", 1 }, // ( > ]

                      { "))", 0 }, // ) == )
                      { ")(", -1 }, // ) < (
                      { ")[", -1 }, // ) < [
                      { ")]", -1 }, // ) < ]

                      { "[[", 0 }, // [ == [
                      { "[]", 0 }, // [ == ]
                      { "[(", -1 }, // [ < (
                      { "[)", 1 }, // [ > )

                      { "]]", 0 }, // ] == ]
                      { "][", 0 }, // ] == [
                      { "](", -1 }, // ] < (
                      { "])", 1 }, // ] > )
                  };

        /// <summary>
        /// 
        /// </summary>
        public static void SelfTest<TBound>()
            where TBound : struct, IComparable<TBound>
        {
            TBound value = default(TBound);

            Point<TBound> a = Point<TBound>.Left(value);
            Point<TBound> b = Point<TBound>.Right(value);

            Debug.Assert(!(a > b));
            Debug.Assert(!(a < b));
            Debug.Assert(a >= b);
            Debug.Assert(a <= b);
            Debug.Assert(a == b);
            Debug.Assert(!(a != b));

            a = Point<TBound>.Left(value);
            b = Point<TBound>.Left(value);

            Debug.Assert(!(a > b));
            Debug.Assert(!(a < b));
            Debug.Assert(a >= b);
            Debug.Assert(a <= b);
            Debug.Assert(a == b);
            Debug.Assert(!(a != b));


            a = Point<TBound>.Right(value);
            b = Point<TBound>.Right(value);

            Debug.Assert(!(a > b));
            Debug.Assert(!(a < b));
            Debug.Assert(a >= b);
            Debug.Assert(a <= b);
            Debug.Assert(a == b);
            Debug.Assert(!(a != b));


            //========================================

            a = Point<TBound>.Left();
            b = Point<TBound>.Right(value);

            Debug.Assert(!(a > b));
            Debug.Assert((a < b));
            Debug.Assert(!(a >= b));
            Debug.Assert(a <= b);
            Debug.Assert(!(a == b));
            Debug.Assert(a != b);

            a = Point<TBound>.Left();
            b = Point<TBound>.Left(value);

            Debug.Assert(!(a > b));
            Debug.Assert((a < b));
            Debug.Assert(!(a >= b));
            Debug.Assert(a <= b);
            Debug.Assert(!(a == b));
            Debug.Assert(a != b);

            //========================================

            a = Point<TBound>.Left(value);
            b = Point<TBound>.Right();

            Debug.Assert(!(a > b));
            Debug.Assert((a < b));
            Debug.Assert(!(a >= b));
            Debug.Assert(a <= b);
            Debug.Assert(!(a == b));
            Debug.Assert(a != b);

            a = Point<TBound>.Right(value);
            b = Point<TBound>.Right();

            Debug.Assert(!(a > b));
            Debug.Assert((a < b));
            Debug.Assert(!(a >= b));
            Debug.Assert(a <= b);
            Debug.Assert(!(a == b));
            Debug.Assert(a != b);


            //========================================

            a = Point<TBound>.Left();
            b = Point<TBound>.Right();

            Debug.Assert(!(a > b));
            Debug.Assert(a < b);
            Debug.Assert(!(a >= b));
            Debug.Assert(a <= b);
            Debug.Assert(!(a == b));
            Debug.Assert(a != b);


            //========================================
            //========================================
            //========================================
            // ([
            a = Point<TBound>.Left(value, false);
            b = Point<TBound>.Left(value);

            Debug.Assert(a > b);
            Debug.Assert(a >= b);
            Debug.Assert(a != b);

            //========================================
            // ((
            a = Point<TBound>.Left(value, false);
            b = Point<TBound>.Left(value, false);

            Debug.Assert(!(a > b));
            Debug.Assert(!(a < b));
            Debug.Assert(a >= b);
            Debug.Assert(a <= b);
            Debug.Assert(a == b);
            Debug.Assert(!(a != b));

            //========================================
            // (]
            a = Point<TBound>.Left(value, false);
            b = Point<TBound>.Right(value);

            Debug.Assert(a > b);
            Debug.Assert(a >= b);
            Debug.Assert(a != b);

            //========================================
            // ()
            a = Point<TBound>.Left(value, false);
            b = Point<TBound>.Right(value, false);

            Debug.Assert(a > b);
            Debug.Assert(!(a < b));
            Debug.Assert(a >= b);
            Debug.Assert(!(a <= b));
            Debug.Assert(!(a == b));
            Debug.Assert(a != b);

            //========================================
            //========================================
            //========================================
            // )[
            a = Point<TBound>.Right(value, false);
            b = Point<TBound>.Left(value);

            Debug.Assert(!(a > b));
            Debug.Assert(a < b);
            Debug.Assert(!(a >= b));
            Debug.Assert(a <= b);
            Debug.Assert(!(a == b));
            Debug.Assert(a != b);

            //========================================
            // )(
            a = Point<TBound>.Right(value, false);
            b = Point<TBound>.Left(value, false);

            Debug.Assert(!(a > b));
            Debug.Assert(a < b);
            Debug.Assert(!(a >= b));
            Debug.Assert(a <= b);
            Debug.Assert(!(a == b));
            Debug.Assert(a != b);

            //========================================
            // )]
            a = Point<TBound>.Right(value, false);
            b = Point<TBound>.Right(value);

            Debug.Assert(!(a > b));
            Debug.Assert(a < b);
            Debug.Assert(!(a >= b));
            Debug.Assert(a <= b);
            Debug.Assert(!(a == b));
            Debug.Assert(a != b);

            //========================================
            // ))
            a = Point<TBound>.Right(value, false);
            b = Point<TBound>.Right(value, false);

            Debug.Assert(!(a > b));
            Debug.Assert(!(a < b));
            Debug.Assert(a >= b);
            Debug.Assert(a <= b);
            Debug.Assert(a == b);
            Debug.Assert(!(a != b));
        }
    }
}