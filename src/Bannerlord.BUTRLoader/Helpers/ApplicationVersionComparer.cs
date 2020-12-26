using System.Collections.Generic;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    /// <summary>
    /// https://github.com/BUTR/Bannerlord.ButterLib/blob/dev/src/Bannerlord.ButterLib/Helpers/ApplicationVersionComparer.cs
    /// </summary>
    internal class ApplicationVersionComparer : IComparer<ApplicationVersion>
    {
        public virtual int Compare(ApplicationVersion x, ApplicationVersion y)
        {
            var majorComparison = x.Major.CompareTo(y.Major);
            if (majorComparison != 0) return majorComparison;

            var minorComparison = x.Minor.CompareTo(y.Minor);
            if (minorComparison != 0) return minorComparison;

            var revisionComparison = x.Revision.CompareTo(y.Revision);
            if (revisionComparison != 0) return revisionComparison;

            var changeSetComparison = x.ChangeSet.CompareTo(y.ChangeSet);
            if (changeSetComparison != 0) return changeSetComparison;

            return 0;
        }
    }
}