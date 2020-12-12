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

            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension1.Movie, LauncherModsPrefabExtension1.XPath, new LauncherModsPrefabExtension1());
            PrefabExtensionManager.RegisterPatch(LauncherModsPrefabExtension2.Movie, LauncherModsPrefabExtension2.XPath, new LauncherModsPrefabExtension2());


            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(WidgetPrefab), nameof(WidgetPrefab.LoadFrom)),
                transpiler: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WidgetPrefabPatch), nameof(WidgetPrefab_LoadFrom_Transpiler))));
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
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => ProcessMovie(null!, null!)))
            });
            return instructionsList.AsEnumerable();
        }
    }
}