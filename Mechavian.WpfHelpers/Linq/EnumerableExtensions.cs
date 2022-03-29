using System;
using System.Collections.Generic;

namespace Mechavian.WpfHelpers.Linq
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> fn)
        {
            foreach (var item in enumerable)
            {
                fn(item);
            }
        }
    }
}