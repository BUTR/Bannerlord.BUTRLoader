using System.Collections.Generic;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Extensions
{
    public static class MBBindingListExtensions
    {
        public static MBBindingList<T> ToMBBindingList<T>(this IEnumerable<T> enumerable)
        {
            var list = new MBBindingList<T>();
            foreach (var entry in enumerable)
            {
                list.Add(entry);
            }
            return list;
        }
    }
}