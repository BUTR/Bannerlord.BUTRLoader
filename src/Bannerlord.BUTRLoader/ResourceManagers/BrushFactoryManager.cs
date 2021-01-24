using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

using TaleWorlds.GauntletUI;

namespace Bannerlord.BUTRLoader.ResourceManagers
{
    public static class BrushFactoryManager
    {
        private static readonly Dictionary<string, Brush> CustomBrushes = new();
        private static readonly List<Func<IEnumerable<Brush>>> DeferredInitialization = new();

        private delegate Brush LoadBrushFromDelegate(BrushFactory instance, XmlNode brushNode);
        private static readonly LoadBrushFromDelegate? LoadBrushFrom =
            AccessTools2.GetDelegate<LoadBrushFromDelegate>(typeof(BrushFactory), "LoadBrushFrom");

        private static Harmony? _harmony;
        private static WeakReference<BrushFactory?> BrushFactoryReference { get; } = new(null);
        public static void SetBrushFactory(BrushFactory brushFactory)
        {
            BrushFactoryReference.SetTarget(brushFactory);

            foreach (var brush in DeferredInitialization.SelectMany(func => func()))
            {
                CustomBrushes[brush.Name] = brush;
            }
        }

        public static IEnumerable<Brush> Create(XmlDocument xmlDocument)
        {
            if (!BrushFactoryReference.TryGetTarget(out var brushFactory) || brushFactory is null)
                yield break;

            foreach (XmlNode brushNode in xmlDocument.SelectSingleNode("Brushes")!.ChildNodes)
            {
                var brush = LoadBrushFrom?.Invoke(brushFactory, brushNode);
                if (brush is not null)
                {
                    yield return brush;
                }
            }
        }
        public static void Register(Func<IEnumerable<Brush>> func) =>DeferredInitialization.Add(func);
        public static void CreateAndRegister(XmlDocument xmlDocument) => Register(() => Create(xmlDocument));

        internal static bool Enable(Harmony harmony)
        {
            _harmony = harmony;

            harmony.Patch(
                SymbolExtensions2.GetPropertyInfo((BrushFactory bf) => bf.Brushes).GetMethod,
                postfix: new HarmonyMethod(AccessTools.Method(typeof(BrushFactoryManager), nameof(GetBrushesPostfix))));

            harmony.Patch(
                SymbolExtensions.GetMethodInfo((BrushFactory bf) => bf.GetBrush(null!)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(BrushFactoryManager), nameof(GetBrushPrefix))));

            var res3 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((BrushFactory bf) => bf.Initialize()),
                prefix: AccessTools.DeclaredMethod(typeof(BrushFactoryManager), nameof(InitializePostfix)));
            if (!res3) return false;

            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void GetBrushesPostfix(ref IEnumerable<Brush> __result)
        {
            __result = __result.Concat(CustomBrushes.Values);
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetBrushPrefix(string name, Dictionary<string, Brush> ____brushes, ref Brush __result)
        {
            if (____brushes.ContainsKey(name) || !CustomBrushes.ContainsKey(name))
                return true;

            if (CustomBrushes[name] is { } brush)
            {
                __result = brush;
                return false;
            }

            return true;
        }


        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializePostfix(ref BrushFactory __instance)
        {
            SetBrushFactory(__instance);

            _harmony?.Unpatch(
                SymbolExtensions.GetMethodInfo((BrushFactory bf) => bf.Initialize()),
                AccessTools.DeclaredMethod(typeof(BrushFactoryManager), nameof(InitializePostfix)));
        }
    }
}