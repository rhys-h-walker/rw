using System;
using System.Collections.Generic;
using System.Text;

namespace Rw.Core.Extensions
{
    public static class IListExtensions
    {
        public static IList<T> AddRange<T>(this IList<T> list, params T[] items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }

            return list;
        }

        public static IList<T> AddRange<T>(this IList<T> list, IList<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }

            return list;
        }
    }
}
