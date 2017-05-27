using System.Collections.Generic;
using System.Linq;

namespace Protoplasm.Utils
{
    public static class ParallelExtender
    {
        public static bool UseParallel;

        public static IEnumerable<T> Parallel<T>(this IEnumerable<T> source)
        {
            return UseParallel ? source.AsParallel() : source;
        }
    }
}