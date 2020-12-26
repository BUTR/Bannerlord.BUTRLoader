using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal class ApplicationVersionFullComparer : ApplicationVersionComparer
    {
        public override int Compare(ApplicationVersion x, ApplicationVersion y)
        {
            var baseComparison = base.Compare(x, y);
            if (baseComparison != 0) return baseComparison;

            var versionGameTypeComparison = x.VersionGameType.CompareTo(y.VersionGameType);
            if (versionGameTypeComparison != 0) return versionGameTypeComparison;

            return 0;
        }
    }
}