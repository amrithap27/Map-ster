﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapster.Utils
{
    public static class MapsterHelper
    {
        public static U GetValueOrDefault<T, U>(IDictionary<T, U> dict, T key)
        {
            return dict.TryGetValue(key, out var value) ? value : default(U);
        }

        public static U FlexibleGet<U>(IDictionary<string, U> dict, string key, Func<string, string> keyConverter)
        {
            return (from kvp in dict
                    where keyConverter(kvp.Key) == key
                    select kvp.Value).FirstOrDefault();
        }

        public static void FlexibleSet<U>(IDictionary<string, U> dict, string key, Func<string, string> keyConverter, U value)
        {
            var dictKey = (from kvp in dict
                           where keyConverter(kvp.Key) == key
                           select kvp.Key).FirstOrDefault();
            dict[dictKey ?? key] = value;
        }

        public static IEnumerable<T> ToEnumerable<T>(IEnumerable<T> source)
        {
            if (source is T[] array)
            {
                for (int i = 0, count = array.Length; i < count; i++)
                {
                    yield return array[i];
                }
            }
            else if (source is IList<T> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    yield return list[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }
    }
}
