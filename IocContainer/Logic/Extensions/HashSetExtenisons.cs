using System.Collections.Generic;

namespace Zt.Containers.Logic.Extensions
{
    internal static class HashSetExtenisons
    {
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                hashSet.Add(item);
            }
        }
    }
}