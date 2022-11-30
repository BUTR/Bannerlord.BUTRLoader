using Bannerlord.BUTRLoader.Helpers;
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
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension1.Movie, UILauncherPrefabExtension1.XPath, new UILauncherPrefabExtension1());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension2.Movie, UILauncherPrefabExtension2.XPath, new UILauncherPrefabExtension2());

            // Options
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension3.Movie, UILauncherPrefabExtension3.XPath, new UILauncherPrefabExtension3());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension4.Movie, UILauncherPrefabExtension4.XPath, new UILauncherPrefabExtension4());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension5.Movie, UILauncherPrefabExtension5.XPath, new UILauncherPrefabExtension5());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension6.Movie, UILauncherPrefabExtension6.XPath, new UILauncherPrefabExtension6());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension7.Movie, UILauncherPrefabExtension7.XPath, new UILauncherPrefabExtension7());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension8.Movie, UILauncherPrefabExtension8.XPath, new UILauncherPrefabExtension8());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension9.Movie, UILauncherPrefabExtension9.XPath, new UILauncherPrefabExtension9());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension10.Movie, UILauncherPrefabExtension10.XPath, new UILauncherPrefabExtension10());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension11.Movie, UILauncherPrefabExtension11.XPath, new UILauncherPrefabExtension11());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension12.Movie, UILauncherPrefabExtension12.XPath, new UILauncherPrefabExtension12());
            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension13.Movie, UILauncherPrefabExtension13.XPath, new UILauncherPrefabExtension13());
            // Options

            PrefabExtensionManager.RegisterPatch(UILauncherPrefabExtension14.Movie, UILauncherPrefabExtension14.XPath, new UILauncherPrefabExtension14());


            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension1.Movie, LauncherModsPrefabExtension1.XPath, new LauncherModsPrefabExtension1());
            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension2.Movie, LauncherModsPrefabExtension2.XPath, new LauncherModsPrefabExtension2());
            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension3.Movie, LauncherModsPrefabExtension3.XPath, new LauncherModsPrefabExtension3());

            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension4.Movie, LauncherModsPrefabExtension4.XPath, new LauncherModsPrefabExtension4());
            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension5.Movie, LauncherModsPrefabExtension5.XPath, new LauncherModsPrefabExtension5());
            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension6.Movie, LauncherModsPrefabExtension6.XPath, new LauncherModsPrefabExtension6());
            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension7.Movie, LauncherModsPrefabExtension7.XPath, new LauncherModsPrefabExtension7());

            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension8.Movie, LauncherModsPrefabExtension8.XPath, new LauncherModsPrefabExtension8());

            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension9.Movie, LauncherModsPrefabExtension9.XPath, new LauncherModsPrefabExtension9());
            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension10.Movie, LauncherModsPrefabExtension10.XPath, new LauncherModsPrefabExtension10());
            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension11.Movie, LauncherModsPrefabExtension11.XPath, new LauncherModsPrefabExtension11());

            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension12.Movie, LauncherModsPrefabExtension12.XPath, new LauncherModsPrefabExtension12());
            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension13.Movie, LauncherModsPrefabExtension13.XPath, new LauncherModsPrefabExtension13());

            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension14.Movie, LauncherModsPrefabExtension14.XPath, new LauncherModsPrefabExtension14());

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> WidgetPrefab_LoadFrom_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
        {
            var instructionsList = instructions.ToList();

            IEnumerable<CodeInstruction> ReturnDefault(string reason)
            {
                return instructionsList.AsEnumerable();
            }

            var constructor = AccessTools2.DeclaredConstructor(typeof(WidgetPrefab));

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

                var instructionList = instructions.ToList();

                var locals = method.GetMethodBody()?.LocalVariables;
                var typeLocal = locals?.FirstOrDefault(x => x.LocalType == typeof(XmlDocument));

                if (typeLocal is null)
                    return returnNull;

                var constructorIndex = -1;
                var constructor = AccessTools2.Constructor(typeof(WidgetPrefab));
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