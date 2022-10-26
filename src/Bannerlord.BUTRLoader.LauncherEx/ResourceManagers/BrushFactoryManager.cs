﻿using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

using TaleWorlds.GauntletUI;

namespace Bannerlord.BUTRLoader.ResourceManagers
{
    internal static class BrushFactoryManager
    {
        private static readonly Dictionary<string, Brush> CustomBrushes = new();
        private static readonly List<Func<IEnumerable<Brush>>> DeferredInitialization = new();

        private delegate Brush LoadBrushFromDelegate(BrushFactory instance, XmlNode brushNode);
        private static readonly LoadBrushFromDelegate? LoadBrushFrom =
            AccessTools2.GetDelegate<LoadBrushFromDelegate>("TaleWorlds.GauntletUI.BrushFactory:LoadBrushFrom");

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
        public static void Register(Func<IEnumerable<Brush>> func) => DeferredInitialization.Add(func);
        public static void CreateAndRegister(XmlDocument xmlDocument) => Register(() => Create(xmlDocument));

        internal static bool Enable(Harmony harmony)
        {
            _harmony = harmony;

            harmony.Patch(
                AccessTools2.PropertyGetter("TaleWorlds.GauntletUI.BrushFactory:Brushes"),
                postfix: new HarmonyMethod(AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(GetBrushesPostfix))));

            harmony.Patch(
                AccessTools2.DeclaredMethod("TaleWorlds.GauntletUI.BrushFactory:GetBrush"),
                new HarmonyMethod(AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(GetBrushPrefix))));

            var res3 = harmony.TryPatch(
                AccessTools2.Method("TaleWorlds.GauntletUI.BrushFactory:LoadBrushes"),
                AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(LoadBrushesPostfix)));
            if (!res3) return false;

            // Preventing inlining Initialize
            harmony.TryPatch(
                AccessTools2.Method("TaleWorlds.GauntletUI.BrushFactory:Initialize"),
                transpiler: AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(BlankTranspiler)));
            // Preventing inlining GetBrush
            harmony.TryPatch(
                AccessTools2.Method("TaleWorlds.GauntletUI.PrefabSystem.ConstantDefinition:GetValue"),
                transpiler: AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools2.Method("TaleWorlds.GauntletUI.PrefabSystem.WidgetExtensions:SetWidgetAttributeFromString"),
                transpiler: AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools2.Method("TaleWorlds.GauntletUI.UIContext:GetBrush"),
                transpiler: AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools2.Method("TaleWorlds.GauntletUI.PrefabSystem.WidgetExtensions:ConvertObject"),
                transpiler: AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(BlankTranspiler)));
            //harmony.TryPatch(
            //    AccessTools2.Method(typeof(BoolBrushChanger), "OnBooleanUpdated"),
            //    transpiler: AccessTools2.Method(typeof(BrushFactoryManager), nameof(BlankTranspiler)));
            // Preventing inlining GetBrush

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
        private static void LoadBrushesPostfix(ref BrushFactory __instance)
        {
            SetBrushFactory(__instance);

            _harmony?.Unpatch(
                AccessTools2.Method("TaleWorlds.GauntletUI.BrushFactory:LoadBrushes"),
                AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(LoadBrushesPostfix)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}