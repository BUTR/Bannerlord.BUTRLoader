using Bannerlord.BUTR.Shared.Extensions;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

using TaleWorlds.TwoDimension.Standalone;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.ResourceManagers
{
    internal static class GraphicsContextManager
    {
        public static GraphicsContext Instance { get; private set; } = default!;

        private static readonly Dictionary<string, OpenGLTexture> Textures = new();
        private static readonly Dictionary<string, Func<OpenGLTexture>> DeferredInitialization = new();

        public static OpenGLTexture Create(byte[] data)
        {
            var path = Path.GetTempFileName();
            File.WriteAllBytes(path, data);
            var openGLTexture = OpenGLTexture.FromFile(path);
            File.Delete(path);

            return openGLTexture;
        }
        public static void Register(string name, Func<OpenGLTexture> func) => DeferredInitialization.Add(name, func);
        public static void CreateAndRegister(string name, byte[] data) => Register(name, () => Create(data));

        internal static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                SymbolExtensions2.GetMethodInfo((GraphicsContext gc) => gc.GetTexture(null!)),
                prefix: AccessTools2.DeclaredMethod(typeof(GraphicsContextManager), nameof(GetTexturePrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                SymbolExtensions2.GetMethodInfo((GraphicsContext gc) => gc.CreateContext(null!)),
                postfix: AccessTools2.DeclaredMethod(typeof(GraphicsContextManager), nameof(CreateContextPostfix)));
            if (!res2) return false;

            // Preventing inlining GetTexture

            // Preventing inlining GetTexture

            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetTexturePrefix(string textureName, ref OpenGLTexture __result)
        {
            if (!Textures.TryGetValue(textureName, out __result))
                return true;
            return false;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CreateContextPostfix(GraphicsContext __instance)
        {
            Instance = __instance;

            foreach (var (name, func) in DeferredInitialization)
            {
                Textures[name] = func();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}