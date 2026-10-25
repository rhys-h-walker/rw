using System;
using System.Collections.Generic;
using System.Text;

namespace Rw.Core.Extensions
{
    public static class IntExtensions
    {
        public static ICollection<T> For<T>(this int num, Func<T> act)
        {
            var col = new List<T>();
            for(var i = 0; i < num; i++)
            {
                col.Add(act.Invoke());
            }

            return col;
        }
    }
}
