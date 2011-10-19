using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace civilsalary.data
{
    public static class LinqExtensions
    {
        //Batching code similar to MoreLinq implementation
        public static IEnumerable<TResult> Batch<TSource,TResult>(this IEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            return BatchImpl(source, size, resultSelector);
        }

        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
        {
            return Batch(source, size, x => x);
        }

        private static IEnumerable<TResult> BatchImpl<TSource, TResult>(this IEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new TSource[size];

                bucket[count++] = item;

                // The bucket is fully buffered before it's yielded
                if (count != size)
                    continue;

                // Select is necessary so bucket contents are streamed too
                yield return resultSelector(bucket.Select(x => x));

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
                yield return resultSelector(bucket.Take(count));
        }

        //TODO: performance enhancement for this?
        /// <summary>
        /// This method flattens the hierarchical object graph by depth ascending.
        /// </summary>
        public static IEnumerable<T> PrioritizedFlatten<T>(this IEnumerable<T> links) where T : IHierarchicalEnumerable<T>
        {
            //TODO: faster way to do this?
            for (var i = 0; i >= 0; i++)
            {
                var found = false;
                foreach (var l in Depth(links, i))
                {
                    found = true;
                    yield return l;
                }
                if (!found)
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// This method flattens the hierarchical object graph.  It fully recurses through a node's children 
        /// before moving to the next sibling node.
        /// </summary>
        static IEnumerable<T> Depth<T>(IEnumerable<T> links, int depth) where T : IHierarchicalEnumerable<T>
        {
            foreach (var l in links)
            {
                if (depth == 0)
                {
                    yield return l;
                }
                else if (depth > 0)
                {
                    foreach (var c in Depth(l.Children, depth - 1))
                    {
                        yield return c;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<HierarchyDepth<T>> Flatten<T>(this IEnumerable<T> links) where T : IHierarchicalEnumerable<T>
        {
            return links.Flatten(0);
        }

        static IEnumerable<HierarchyDepth<T>> Flatten<T>(this IEnumerable<T> links, int depth) where T : IHierarchicalEnumerable<T>
        {
            foreach (var l in links)
            {
                yield return new HierarchyDepth<T>(l, depth);

                foreach (var c in l.Children.Flatten(depth + 1))
                {
                    yield return c;
                }
            }
        }

        public static double? Median<T>(this ICollection<T> list, Func<T, double?> valueSelector)
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

    public sealed class HierarchyDepth<T>
    {
        public HierarchyDepth(T item, int depth)
        {
            Item = item;
            Depth = depth;
        }

        public T Item { get; private set; }
        public int Depth { get; private set; }
    }
}
