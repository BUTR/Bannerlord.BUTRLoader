using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class ViewModelPatch
    {
        public static void Enable(Harmony harmony)
        {
            harmony.TryPatch(
                AccessTools2.DeclaredMethod("TaleWorlds.Library.ViewModel:ExecuteCommand"),
                transpiler: AccessTools2.DeclaredMethod(typeof(ViewModelPatch), nameof(ViewModel_ExecuteCommand_Transpiler)));

            // Preventing inlining ExecuteCommand
            harmony.TryPatch(
                AccessTools2.DeclaredMethod("TaleWorlds.GauntletUI.Data.GauntletView:OnCommand"),
                transpiler: AccessTools2.DeclaredMethod(typeof(ViewModelPatch), nameof(BlankTranspiler)));
            // Preventing inlining ExecuteCommand
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> ViewModel_ExecuteCommand_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            var instructionList = instructions.ToList();

            var jmpOriginalFlow = ilGenerator.DefineLabel();
            instructionList[0].labels.Add(jmpOriginalFlow);

            instructionList.InsertRange(0, new List<CodeInstruction>
                {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Ldarg_2),
                    new(OpCodes.Call, AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.ViewModelPatch:ExecuteCommand")),
                    new(OpCodes.Brtrue, jmpOriginalFlow),
                    new(OpCodes.Ret)
                });
            return instructionList;
        }
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool ExecuteCommand(ViewModel viewModel, string commandName, params object[] parameters)
        {
            static object? ConvertValueTo(string value, Type parameterType)
            {
                if (parameterType == typeof(string))
                    return value;
                if (parameterType == typeof(int))
                    return Convert.ToInt32(value);
                if (parameterType == typeof(float))
                    return Convert.ToSingle(value);
                return null;
            }

            if (MixinManager.Mixins.TryGetValue(viewModel, out var mixins))
            {
                foreach (var mixin in mixins)
                {
                    var method = mixin.GetType().GetMethod(commandName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method is null)
                        continue;

                    if (method.GetParameters() is { } methodParameters && methodParameters.Length == parameters.Length)
                    {
                        var array = new object?[parameters.Length];
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            var methodParameterType = methodParameters[i].ParameterType;

                            var obj = parameters[i];
                            array[i] = obj;
                            if (obj is string str && methodParameterType != typeof(string))
                            {
                                array[i] = ConvertValueTo(str, methodParameterType);
                            }
                        }

                        method.InvokeWithLog(mixin, array);
                        return false;
                    }

                    if (method.GetParameters().Length == 0)
                    {
                        method.InvokeWithLog(mixin, null);
                        return false;
                    }
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}