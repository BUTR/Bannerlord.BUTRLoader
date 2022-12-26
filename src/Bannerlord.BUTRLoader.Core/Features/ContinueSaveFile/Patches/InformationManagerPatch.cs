using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Features.ContinueSaveFile.Patches
{
    internal static class InformationManagerPatch
    {
        internal static bool SkipChange = false;

        public static bool Enable(Harmony harmony)
        {
            return true &
                   harmony.TryPatch(
                       original: AccessTools2.Method(typeof(InformationManager), "ShowInquiry"),
                       prefix: AccessTools2.Method(typeof(InformationManagerPatch), nameof(Prefix)));
        }

        private static bool Prefix(InquiryData data)
        {
            if (SkipChange)
            {
                data.AffirmativeAction?.Invoke();
                return false;
            }
            return true;
        }
    }
}