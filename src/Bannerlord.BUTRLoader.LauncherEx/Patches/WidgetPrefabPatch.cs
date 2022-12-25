﻿using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches.PrefabExtensions;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml;

using TaleWorlds.GauntletUI.PrefabSystem;

namespace Bannerlord.BUTRLoader.Patches
{
    // https://github.com/BUTR/Bannerlord.UIExtenderEx/blob/dev/src/Bannerlord.UIExtenderEx/Patches/WidgetPrefabPatch.cs
    internal static class WidgetPrefabPatch
    {
        public static bool Enable(Harmony harmony)
        {
            // Options
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension3.Movie, UILauncherPrefabExtension3.XPath, new UILauncherPrefabExtension3());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension4.Movie, UILauncherPrefabExtension4.XPath, new UILauncherPrefabExtension4());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension5.Movie, UILauncherPrefabExtension5.XPath, new UILauncherPrefabExtension5());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension6.Movie, UILauncherPrefabExtension6.XPath, new UILauncherPrefabExtension6());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension20.Movie, UILauncherPrefabExtension20.XPath, new UILauncherPrefabExtension20());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension21.Movie, UILauncherPrefabExtension21.XPath, new UILauncherPrefabExtension21());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension7.Movie, UILauncherPrefabExtension7.XPath, new UILauncherPrefabExtension7());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension22.Movie, UILauncherPrefabExtension22.XPath, new UILauncherPrefabExtension22());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension23.Movie, UILauncherPrefabExtension23.XPath, new UILauncherPrefabExtension23());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension8.Movie, UILauncherPrefabExtension8.XPath, new UILauncherPrefabExtension8());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension9.Movie, UILauncherPrefabExtension9.XPath, new UILauncherPrefabExtension9());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension10.Movie, UILauncherPrefabExtension10.XPath, new UILauncherPrefabExtension10());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension11.Movie, UILauncherPrefabExtension11.XPath, new UILauncherPrefabExtension11());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension12.Movie, UILauncherPrefabExtension12.XPath, new UILauncherPrefabExtension12());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension13.Movie, UILauncherPrefabExtension13.XPath, new UILauncherPrefabExtension13());
            // Options


            // Import/Export
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension14.Movie, UILauncherPrefabExtension14.XPath, new UILauncherPrefabExtension14());
            // Import/Export

            // Minor
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension1.Movie, UILauncherPrefabExtension1.XPath, new UILauncherPrefabExtension1());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension15.Movie, UILauncherPrefabExtension15.XPath, new UILauncherPrefabExtension15());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension16.Movie, UILauncherPrefabExtension16.XPath, new UILauncherPrefabExtension16());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension17.Movie, UILauncherPrefabExtension17.XPath, new UILauncherPrefabExtension17());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension18.Movie, UILauncherPrefabExtension18.XPath, new UILauncherPrefabExtension18());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension19.Movie, UILauncherPrefabExtension19.XPath, new UILauncherPrefabExtension19());
            // Minor

            // Compact
            PrefabExtensionManager.RegisterPatch(ModuleTuplePrefabExtension4.Movie, ModuleTuplePrefabExtension4.XPath, new ModuleTuplePrefabExtension4());
            PrefabExtensionManager.RegisterPatch(ModuleTuplePrefabExtension5.Movie, ModuleTuplePrefabExtension5.XPath, new ModuleTuplePrefabExtension5());
            PrefabExtensionManager.RegisterPatch(ModuleTuplePrefabExtension6.Movie, ModuleTuplePrefabExtension6.XPath, new ModuleTuplePrefabExtension6());
            PrefabExtensionManager.RegisterPatch(ModuleTuplePrefabExtension7.Movie, ModuleTuplePrefabExtension7.XPath, new ModuleTuplePrefabExtension7());
            // Compact

            var res1 = harmony.TryPatch(
                AccessTools2.DeclaredMethod(typeof(WidgetPrefab), "LoadFrom"),
                transpiler: AccessTools2.DeclaredMethod(typeof(WidgetPrefabPatch), nameof(WidgetPrefab_LoadFrom_Transpiler)));
            if (!res1) return false;

            var res2 = harmony.TryCreateReversePatcher(
                AccessTools2.DeclaredMethod(typeof(WidgetPrefab), "LoadFrom"),
                AccessTools2.DeclaredMethod(typeof(WidgetPrefabPatch), nameof(LoadFromDocument)));
            if (res2 is null) return false;
            res2.Patch();

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ProcessMovie(string path, XmlDocument document)
        {
            var movieName = Path.GetFileNameWithoutExtension(path);
            PrefabExtensionManager.ProcessMovieIfNeeded(movieName, document);
        }

        private static int GetWidgetPrefabConstructorIndex(IList<CodeInstruction> instructions, MethodBase originalMethod)
        {
            var constructor = AccessTools2.DeclaredConstructor(typeof(WidgetPrefab));

            var locals = originalMethod.GetMethodBody()?.LocalVariables;
            var widgetPrefabLocal = locals?.FirstOrDefault(x => x.LocalType == typeof(WidgetPrefab));

            if (widgetPrefabLocal is null)
                return -1;

            var constructorIndex = -1;
            for (var i = 0; i < instructions.Count - 2; i++)
            {
                if (instructions[i + 0].opcode != OpCodes.Newobj || !Equals(instructions[i + 0].operand, constructor))
                    continue;

                if (!instructions[i + 1].IsStloc())
                    continue;

                constructorIndex = i;
                break;
            }
            return constructorIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> WidgetPrefab_LoadFrom_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
        {
            var instructionsList = instructions.ToList();

            IEnumerable<CodeInstruction> ReturnDefault(string reason)
            {
                return instructionsList.AsEnumerable();
            }

            var widgetPrefabConstructorIndex = GetWidgetPrefabConstructorIndex(instructionsList, method);
            if (widgetPrefabConstructorIndex == -1)
                return ReturnDefault("Pattern not found");

            // ProcessMovie(path, xmlDocument);
            instructionsList.InsertRange(widgetPrefabConstructorIndex + 1, new List<CodeInstruction>
            {
                new (OpCodes.Ldarg_2),
                new (OpCodes.Ldloc_0),
                new (OpCodes.Call, AccessTools2.DeclaredMethod(typeof(WidgetPrefabPatch), nameof(ProcessMovie)))
            });
            return instructionsList.AsEnumerable();
        }


        // We can call a slightly modified native game call this way
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static WidgetPrefab LoadFromDocument(PrefabExtensionContext prefabExtensionContext, WidgetAttributeContext widgetAttributeContext, string path, XmlDocument document)
        {
            // Replaces reading XML from file with assigning it from the new local variable `XmlDocument document`
            [MethodImpl(MethodImplOptions.NoInlining)]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
            {
                var returnNull = new List<CodeInstruction>
                {
                    new (OpCodes.Ldnull),
                    new (OpCodes.Ret)
                }.AsEnumerable();

                var instructionsList = instructions.ToList();

                var locals = method.GetMethodBody()?.LocalVariables;
                var xmlDocumentLocal = locals?.FirstOrDefault(x => x.LocalType == typeof(XmlDocument));

                if (xmlDocumentLocal is null)
                    return returnNull;

                var widgetPrefabConstructorIndex = GetWidgetPrefabConstructorIndex(instructionsList, method);
                if (widgetPrefabConstructorIndex == -1)
                    return returnNull;

                for (var i = 0; i < widgetPrefabConstructorIndex; i++)
                {
                    instructionsList[i] = new CodeInstruction(OpCodes.Nop);
                }

                instructionsList[widgetPrefabConstructorIndex - 2] = new CodeInstruction(OpCodes.Ldarg_S, 3);
                instructionsList[widgetPrefabConstructorIndex - 1] = new CodeInstruction(OpCodes.Stloc_S, xmlDocumentLocal.LocalIndex);

                // ProcessMovie(path, xmlDocument);
                instructionsList.InsertRange(widgetPrefabConstructorIndex + 1, new List<CodeInstruction>
                {
                    new (OpCodes.Ldarg_S, 2),
                    new (OpCodes.Ldloc_S, xmlDocumentLocal.LocalIndex),
                    new (OpCodes.Call, AccessTools2.DeclaredMethod(typeof(WidgetPrefabPatch), nameof(ProcessMovie)))
                });


                return instructionsList.AsEnumerable();
            }

            // make compiler happy
            _ = Transpiler(null!, null!);

            // make analyzer happy
            prefabExtensionContext.AddExtension(null);
            widgetAttributeContext.RegisterKeyType(null);
            path.Do(null);
            document.Validate(null);

            // make compiler happy
            return null!;
        }
    }
}