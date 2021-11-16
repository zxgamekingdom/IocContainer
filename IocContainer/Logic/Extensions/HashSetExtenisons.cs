using System.Collections.Generic;

namespace IocContainer.Logic.Extensions
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