using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches.PrefabExtensions;

using HarmonyLib;

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
        public static void Enable(Harmony harmony)
        {
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension1.Movie, UILauncherPrefabExtension1.XPath, new UILauncherPrefabExtension1());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension2.Movie, UILauncherPrefabExtension2.XPath, new UILauncherPrefabExtension2());

            // Options
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension3.Movie , UILauncherPrefabExtension3.XPath , new UILauncherPrefabExtension3 ());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension4.Movie , UILauncherPrefabExtension4.XPath , new UILauncherPrefabExtension4 ());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension5.Movie , UILauncherPrefabExtension5.XPath , new UILauncherPrefabExtension5 ());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension6.Movie , UILauncherPrefabExtension6.XPath , new UILauncherPrefabExtension6 ());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension7.Movie , UILauncherPrefabExtension7.XPath , new UILauncherPrefabExtension7 ());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension8.Movie , UILauncherPrefabExtension8.XPath , new UILauncherPrefabExtension8 ());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension9.Movie , UILauncherPrefabExtension9.XPath , new UILauncherPrefabExtension9 ());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension10.Movie, UILauncherPrefabExtension10.XPath, new UILauncherPrefabExtension10());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension11.Movie, UILauncherPrefabExtension11.XPath, new UILauncherPrefabExtension11());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension12.Movie, UILauncherPrefabExtension12.XPath, new UILauncherPrefabExtension12());
            // Options

            //PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension1.Movie, LauncherModsPrefabExtension1.XPath, new LauncherModsPrefabExtension1());
            //PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension2.Movie, LauncherModsPrefabExtension2.XPath, new LauncherModsPrefabExtension2());


            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(WidgetPrefab), nameof(WidgetPrefab.LoadFrom)),
                transpiler: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WidgetPrefabPatch), nameof(WidgetPrefab_LoadFrom_Transpiler))));

            harmony.CreateReversePatcher(
                SymbolExtensions.GetMethodInfo(() => WidgetPrefab.LoadFrom(null!, null!, null!)),
                new HarmonyMethod(SymbolExtensions.GetMethodInfo(() => LoadFromDocument(null!, null!, null!, null!)))).Patch();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ProcessMovie(string path, XmlDocument document)
        {
            var movieName = Path.GetFileNameWithoutExtension(path);
            PrefabExtensionManager.ProcessMovieIfNeeded(movieName, document);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> WidgetPrefab_LoadFrom_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
        {
            var instructionsList = instructions.ToList();

            IEnumerable<CodeInstruction> ReturnDefault(string reason)
            {
                return instructionsList.AsEnumerable();
            }

            var constructor = AccessTools.DeclaredConstructor(typeof(WidgetPrefab));

            var locals = method.GetMethodBody()?.LocalVariables;
            var typeLocal = locals?.FirstOrDefault(x => x.LocalType == typeof(WidgetPrefab));

            if (typeLocal is null)
                return ReturnDefault("Local not found");

            var startIndex = -1;
            for (var i = 0; i < instructionsList.Count - 2; i++)
            {
                if (instructionsList[i + 0].opcode != OpCodes.Newobj || !Equals(instructionsList[i + 0].operand, constructor))
                    continue;

                if (!instructionsList[i + 1].IsStloc())
                    continue;

                startIndex = i;
                break;
            }

            if (startIndex == -1)
                return ReturnDefault("Pattern not found");

            // ProcessMovie(path, xmlDocument);
            instructionsList.InsertRange(startIndex + 1, new List<CodeInstruction>
            {
                new (OpCodes.Ldarg_2),
                new (OpCodes.Ldloc_0),
                new (OpCodes.Call, SymbolExtensions.GetMethodInfo(() => ProcessMovie(null!, null!)))
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

                var instructionList = instructions.ToList();

                var locals = method.GetMethodBody()?.LocalVariables;
                var typeLocal = locals?.FirstOrDefault(x => x.LocalType == typeof(XmlDocument));

                if (typeLocal is null)
                    return returnNull;

                var constructorIndex = -1;
                var constructor = AccessTools.Constructor(typeof(WidgetPrefab));
                for (var i = 0; i < instructionList.Count; i++)
                {
                    if (instructionList[i].opcode == OpCodes.Newobj && Equals(instructionList[i].operand, constructor))
                        constructorIndex = i;
                }

                if (constructorIndex == -1)
                    return returnNull;

                for (var i = 0; i < constructorIndex; i++)
                {
                    instructionList[i] = new CodeInstruction(OpCodes.Nop);
                }

                instructionList[constructorIndex - 2] = new CodeInstruction(OpCodes.Ldarg_S, 3);
                instructionList[constructorIndex - 1] = new CodeInstruction(OpCodes.Stloc_S, typeLocal.LocalIndex);

                return instructionList.AsEnumerable();
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