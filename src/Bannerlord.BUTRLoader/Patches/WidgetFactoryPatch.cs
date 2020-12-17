using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

using TaleWorlds.GauntletUI.PrefabSystem;

namespace Bannerlord.BUTRLoader.Patches
{
    // TODO: Backport
    internal static class WidgetFactoryPost154Patch
    {
        private static readonly Dictionary<string, string> CustomTypePaths = new();
        private static readonly Dictionary<string, WidgetPrefab> LiveCustomTypes = new();
        private static readonly Dictionary<string, int> LiveInstanceTracker = new();

        private static Func<string, WidgetPrefab?>? _widgetRequested;

        public static void Enable(Harmony harmony, Func<string, WidgetPrefab?> widgetRequested)
        {
            _widgetRequested = widgetRequested;

            LauncherUIPatch.OnInitialize += LauncherUIPatch_OnInitialize;

            harmony.Patch(
                SymbolExtensions.GetMethodInfo((WidgetFactory wf) => wf.GetCustomType(null!)),
                prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WidgetFactoryPost154Patch), nameof(GetCustomTypePrefix))));

            harmony.Patch(
                SymbolExtensions.GetMethodInfo((WidgetFactory wf) => wf.GetWidgetTypes()),
                prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WidgetFactoryPost154Patch), nameof(GetWidgetTypesPostfix))));

            harmony.Patch(
                SymbolExtensions.GetMethodInfo((WidgetFactory wf) => wf.IsCustomType(null!)),
                prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WidgetFactoryPost154Patch), nameof(IsCustomTypePrefix))));

            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(WidgetFactory), "OnUnload"),
                prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WidgetFactoryPost154Patch), nameof(OnUnloadPrefix))));
        }

        private static void LauncherUIPatch_OnInitialize(object sender, EventArgs e)
        {
            PrefabInjector.Register("Launcher.Options");
            CustomTypePaths.Add("Launcher.Options", "");
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void GetWidgetTypesPostfix(ref IEnumerable<string> __result)
        {
            __result = __result.Concat(CustomTypePaths.Keys);
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsCustomTypePrefix(string typeName, ref bool __result)
        {
            if (!CustomTypePaths.ContainsKey(typeName))
                return true;

            __result = true;
            return false;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetCustomTypePrefix(string typeName, IDictionary ____liveCustomTypes, ref WidgetPrefab __result)
        {
            if (____liveCustomTypes.Contains(typeName) || !CustomTypePaths.ContainsKey(typeName))
                return true;

            if (LiveCustomTypes.TryGetValue(typeName, out var liveWidgetPrefab))
            {
                LiveInstanceTracker[typeName]++;
                __result = liveWidgetPrefab;
                return false;
            }

            if (_widgetRequested?.Invoke(typeName) is { } widgetPrefab)
            {
                LiveCustomTypes.Add(typeName, widgetPrefab);
                LiveInstanceTracker[typeName] = 1;

                __result = widgetPrefab;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool OnUnloadPrefix(string typeName)
        {
            if (LiveCustomTypes.ContainsKey(typeName))
            {
                LiveInstanceTracker[typeName]--;
                if (LiveInstanceTracker[typeName] == 0)
                {
                    LiveCustomTypes.Remove(typeName);
                    LiveInstanceTracker.Remove(typeName);
                }
                return false;
            }

            return true;
        }
    }
}