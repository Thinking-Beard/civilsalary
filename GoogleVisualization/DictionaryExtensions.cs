using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoogleVisualization
{
    public static class DictionaryExtensions
    {
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> mergeTo, IDictionary<TKey, TValue> mergeFrom)
        {
            foreach (var kvp in mergeFrom)
            {
                mergeTo[kvp.Key] = kvp.Value;
            }
        }
    }
}
