using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSample.Utilities
{
    public static class Extensions
    {
        public static Dictionary<TKey, TSource> ToSafeDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>();
            foreach (TSource s in source)
            {
                TKey key = keySelector(s);
                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, s);
            }

            return dictionary;
        }
    }
}
