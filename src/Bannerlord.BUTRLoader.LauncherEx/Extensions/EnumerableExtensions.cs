using System.Collections.Generic;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Extensions
{
    public static class EnumerableExtensions
    {
        public readonly struct EnumerableMetadata<T>
        {
            public T Value { get; private init; }
            public int Index { get; private init; }
            public bool IsFirst { get; private init; }
            public bool IsLast { get; private init; }

            public EnumerableMetadata(T value, int index, bool isFirst, bool isLast) : this()
            {
                Value = value;
                Index = index;
                IsFirst = isFirst;
                IsLast = isLast;
            }
        }

        public static MBBindingList<T> ToMBBindingList<T>(this IEnumerable<T> enumerable)
        {
            var list = new MBBindingList<T>();
            foreach (var entry in enumerable)
            {
                list.Add(entry);
            }
            return list;
        }

        public static IEnumerable<EnumerableMetadata<T>> WithMetadata<T>(this IEnumerable<T> elements)
        {
            using var enumerator = elements.GetEnumerator();
            var isFirst = true;
            var hasNext = enumerator.MoveNext();
            var index = 0;
            while (hasNext)
            {
                var current = enumerator.Current;
                hasNext = enumerator.MoveNext();
                yield return new EnumerableMetadata<T>(current, index, isFirst, !hasNext);
                isFirst = false;
                index++;
            }
        }
    }
}