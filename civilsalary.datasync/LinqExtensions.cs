using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace civilsalary.datasync
{
    static class LinqExtensions
    {
        public static decimal? Median<T>(this ICollection<T> list, Func<T, decimal?> valueSelector)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (valueSelector == null) throw new ArgumentNullException("value");

            if (list.Count == 0) return null;

            var midpoint = (list.Count - 1) / 2;
            var sorted = list.Select(valueSelector).OrderBy(v => v).Skip(midpoint > 0 ? midpoint - 1 : 0);

            if (list.Count % 2 == 0)
            {
                return sorted.Take(2).Average();
            }

            return sorted.First();
        }
    }
}
