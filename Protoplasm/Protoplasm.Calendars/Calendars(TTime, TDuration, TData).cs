using System;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
        where TTime : struct, IComparable<TTime>
        where TDuration : struct, IComparable<TDuration>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public delegate TData Addition(TData a, TData b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public delegate TData Substraction(TData a, TData b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public delegate string ToDebugString(TData data);

    }
}