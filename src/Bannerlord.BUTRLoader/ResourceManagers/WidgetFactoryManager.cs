using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Patches;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.PrefabSystem;

namespace Bannerlord.BUTRLoader.ResourceManagers
{
    /// <summary>
    /// https://github.com/Aragas/Bannerlord.MBOptionScreen/blob/dev/src/MCM.UI/Patches/WidgetFactoryManager.cs
    /// </summary>
    internal static class WidgetFactoryManager
    {
        private static readonly AccessTools.FieldRef<WidgetFactory, IDictionary>? _liveCustomTypes =
            AccessTools2.FieldRefAccess<WidgetFactory, IDictionary>("_liveCustomTypes");

        private static readonly Dictionary<string, Func<WidgetPrefab?>> CustomTypes = new();
        private static readonly Dictionary<string, Type> BuiltinTypes = new();
        private static readonly Dictionary<string, WidgetPrefab> LiveCustomTypes = new();
        private static readonly Dictionary<string, int> LiveInstanceTracker = new();

        private static Harmony? _harmony;
        private static WeakReference<WidgetFactory?> WidgetFactoryReference { get; } = new(null);
        public static void SetWidgetFactory(WidgetFactory widgetFactory)
        {
            WidgetFactoryReference.SetTarget(widgetFactory);
        }

        public static WidgetPrefab? Create(XmlDocument doc)
        {
            if (!WidgetFactoryReference.TryGetTarget(out var widgetFactory) || widgetFactory is null)
                return null;

            return WidgetPrefabPatch.LoadFromDocument(
                widgetFactory.PrefabExtensionContext,
                widgetFactory.WidgetAttributeContext,
                string.Empty,
                doc);
        }
        public static void Register(Type widgetType)
        {
            BuiltinTypes[widgetType.Name] = widgetType;
            WidgetInfo.ReLoad();
        }
        public static void Register(string name, Func<WidgetPrefab?> create) => CustomTypes.Add(name, create);
        public static void CreateAndRegister(string name, XmlDocument xmlDocument) => Register(name, () => Create(xmlDocument));

        public static bool Enable(Harmony harmony)
        {
            _harmony = harmony;

            var res1 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((WidgetFactory wf) => wf.GetCustomType(null!)),
                prefix: AccessTools.DeclaredMethod(typeof(WidgetFactoryManager), nameof(GetCustomTypePrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((WidgetFactory wf) => wf.GetWidgetTypes()),
                prefix: AccessTools.DeclaredMethod(typeof(WidgetFactoryManager), nameof(GetWidgetTypesPostfix)));
            if (!res2) return false;

            var res3 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((WidgetFactory wf) => wf.IsCustomType(null!)),
                prefix: AccessTools.DeclaredMethod(typeof(WidgetFactoryManager), nameof(IsCustomTypePrefix)));
            if (!res3) return false;

            var res4 = harmony.TryPatch(
                AccessTools.DeclaredMethod(typeof(WidgetFactory), "OnUnload"),
                prefix: AccessTools.DeclaredMethod(typeof(WidgetFactoryManager), nameof(OnUnloadPrefix)));
            //if (!res4) return false;

            var res5 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((WidgetFactory wf) => wf.Initialize()),
                prefix: AccessTools.DeclaredMethod(typeof(WidgetFactoryManager), nameof(InitializePostfix)));
            if (!res5) return false;

            var res6 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((WidgetFactory wf) => wf.CreateBuiltinWidget(null!, null!)),
                prefix: AccessTools.DeclaredMethod(typeof(WidgetFactoryManager), nameof(CreateBuiltinWidgetPrefix)));
            if (!res6) return false;

            // GetCustomType is too complex to be inlined
            // CreateBuiltinWidget is too complex to be inlined
            // GetWidgetTypes is not used?
            // Preventing inlining IsCustomType
            harmony.TryPatch(
                AccessTools.Method(typeof(WidgetTemplate), "CreateWidgets"),
                transpiler: AccessTools.Method(typeof(WidgetFactoryManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(WidgetTemplate), "OnRelease"),
                transpiler: AccessTools.Method(typeof(WidgetFactoryManager), nameof(BlankTranspiler)));
            // Preventing inlining GetCustomType
            //harmony.Patch(
            //    AccessTools.Method(typeof(GauntletMovie), "LoadMovie"),
            //    transpiler: new HarmonyMethod(AccessTools.Method(typeof(WidgetFactoryManager), nameof(BlankTranspiler))));

            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void GetWidgetTypesPostfix(ref IEnumerable<string> __result)
        {
            __result = __result.Concat(BuiltinTypes.Keys).Concat(CustomTypes.Keys);
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool CreateBuiltinWidgetPrefix(UIContext context, string typeName, ref Widget? __result)
        {
            if (!BuiltinTypes.TryGetValue(typeName, out var type))
                return true;

            __result = type.GetConstructor(AccessTools.all, null, new[] { typeof(UIContext) }, null)?.Invoke(new object[] { context }) as Widget;
            return false;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsCustomTypePrefix(string typeName, ref bool __result)
        {
            if (!CustomTypes.ContainsKey(typeName))
                return true;

            __result = true;
            return false;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetCustomTypePrefix(WidgetFactory __instance, string typeName, ref WidgetPrefab __result)
        {
            if (_liveCustomTypes?.Invoke(__instance) is { } ____liveCustomTypes &&
                ____liveCustomTypes.Contains(typeName) || !CustomTypes.ContainsKey(typeName))
                return true;

            if (LiveCustomTypes.TryGetValue(typeName, out var liveWidgetPrefab))
            {
                LiveInstanceTracker[typeName]++;
                __result = liveWidgetPrefab;
                return false;
            }

            if (CustomTypes[typeName]?.Invoke() is { } widgetPrefab)
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

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializePostfix(ref WidgetFactory __instance)
        {
            SetWidgetFactory(__instance);

            _harmony?.Unpatch(
                SymbolExtensions.GetMethodInfo((WidgetFactory wf) => wf.Initialize()),
                AccessTools.DeclaredMethod(typeof(WidgetFactoryManager), nameof(InitializePostfix)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}