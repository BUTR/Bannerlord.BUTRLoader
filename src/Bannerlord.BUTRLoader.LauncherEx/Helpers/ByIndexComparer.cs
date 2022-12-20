using System;
using System.Collections.Generic;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class ByIndexComparer<TItem> : IComparer<TItem>
    {
        private readonly Func<TItem, int> _getIndex;

        public ByIndexComparer(Func<TItem, int> getIndex)
        {
            _getIndex = getIndex;
        }

        public int Compare(TItem x, TItem y)
        {
            var xIdx = _getIndex(x);
            var yIdx = _getIndex(y);
            if (xIdx == yIdx) return 0;
            if (xIdx > yIdx) return 1;
            if (xIdx < yIdx) return -1;
            return 0;
        }
    }
}