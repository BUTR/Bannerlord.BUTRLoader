using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.TwoDimension;

namespace Bannerlord.BUTRLoader.ResourceManagers
{
    internal static class SpriteDataManager
    {
        internal sealed class SpriteFromTexture : Sprite
        {
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float> FieldMapX =
                AccessTools.StructFieldRefAccess<SpriteDrawData, float>("MapX");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float> FieldMapY =
                AccessTools.StructFieldRefAccess<SpriteDrawData, float>("MapY");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float> FieldScale =
                AccessTools.StructFieldRefAccess<SpriteDrawData, float>("Scale");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float> FieldWidth =
                AccessTools.StructFieldRefAccess<SpriteDrawData, float>("Width");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float> FieldHeight =
                AccessTools.StructFieldRefAccess<SpriteDrawData, float>("Height");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, bool> FieldHorizontalFlip =
                AccessTools.StructFieldRefAccess<SpriteDrawData, bool>("HorizontalFlip");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, bool> FieldVerticalFlip =
                AccessTools.StructFieldRefAccess<SpriteDrawData, bool>("VerticalFlip");

            private static readonly Type Vector2 = Type.GetType("System.Numerics.Vector2, System.Numerics.Vectors");
            private static readonly ConstructorInfo Vector2Constructor = AccessTools.Constructor(Vector2, new [] { typeof(float), typeof(float) });
            private static readonly MethodInfo CreateQuad = AccessTools.Method(typeof(DrawObject2D), "CreateQuad");


            public override Texture Texture { get; }

            private readonly float[] _vertices;
            private readonly float[] _uvs;
            private readonly uint[] _indices;

            public SpriteFromTexture(Texture texture) : this("Sprite", texture) { }
            public SpriteFromTexture(string name, Texture texture) : base(name, texture.Width, texture.Height)
            {
                Texture = texture;
                _vertices = new float[8];
                _uvs = new float[8];
                _indices = new uint[6];
                _indices[0] = 0U;
                _indices[1] = 1U;
                _indices[2] = 2U;
                _indices[3] = 0U;
                _indices[4] = 2U;
                _indices[5] = 3U;
            }

            public override float GetScaleToUse(float width, float height, float scale) => scale;

            protected override DrawObject2D GetArrays(SpriteDrawData spriteDrawData)
            {
                if (CachedDrawObject != null && CachedDrawData == spriteDrawData)
                    return CachedDrawObject;

                var mapX = FieldMapX(ref spriteDrawData);
                var mapY = FieldMapY(ref spriteDrawData);
                var width = FieldWidth(ref spriteDrawData);
                var height = FieldHeight(ref spriteDrawData);
                var horizontalFlip = FieldHorizontalFlip(ref spriteDrawData);
                var verticalFlip = FieldVerticalFlip(ref spriteDrawData);

                //var vec2 = Vector2Constructor.Invoke(new object[] { width, height });
                //var quad1 = CreateQuad.Invoke(null, new object[]{ vec2 }) as DrawObject2D;
                //return quad1;
                //var quad = DrawObject2D.CreateQuad(new Vector2(width, height));
                //return quad;

                if (mapX == 0f && mapY == 0f)
                {
                    PopulateVertices(Texture, mapX, mapY, _vertices, 0, 1f, width, height);
                    PopulateTextureCoordinates(_uvs, 0, horizontalFlip, verticalFlip);
                    var drawObject2D = new DrawObject2D(MeshTopology.Triangles, _vertices.ToArray(), _uvs.ToArray(), _indices.ToArray(), 6)
                    {
                        DrawObjectType = DrawObjectType.Quad,
                        Width = width,
                        Height = height,
                        MinU = 0f,
                        MaxU = 1f,
                        MinV = 0f,
                        MaxV = 1f
                    };
                    if (horizontalFlip)
                    {
                        drawObject2D.MinU = 1f;
                        drawObject2D.MaxU = 0f;
                    }
                    if (verticalFlip)
                    {
                        drawObject2D.MinV = 1f;
                        drawObject2D.MaxV = 0f;
                    }

                    CachedDrawData = spriteDrawData;
                    CachedDrawObject = drawObject2D;
                    return drawObject2D;
                }

                PopulateVertices(Texture, mapX, mapY, _vertices, 0, 1f, width, height);
                PopulateTextureCoordinates(_uvs, 0, horizontalFlip, verticalFlip);
                var drawObject2D2 = new DrawObject2D(MeshTopology.Triangles, _vertices.ToArray(), _uvs.ToArray(), _indices.ToArray(), 6)
                {
                    DrawObjectType = DrawObjectType.Mesh
                };

                CachedDrawData = spriteDrawData;
                CachedDrawObject = drawObject2D2;
                return drawObject2D2;
            }

            private static void PopulateVertices(Texture texture, float screenX, float screenY, float[] outVertices, int verticesStartIndex, float scale, float customWidth, float customHeight)
            {
                var widthProp = customWidth / texture.Width;
                var heightProp = customHeight / texture.Height;
                var widthScaled = texture.Width * scale * widthProp;
                var heightScaled = texture.Height * scale * heightProp;

                outVertices[verticesStartIndex] = screenX + 0f;
                outVertices[verticesStartIndex + 1] = screenY + 0f;
                outVertices[verticesStartIndex + 2] = screenX + 0f;
                outVertices[verticesStartIndex + 3] = screenY + heightScaled;
                outVertices[verticesStartIndex + 4] = screenX + widthScaled;
                outVertices[verticesStartIndex + 5] = screenY + heightScaled;
                outVertices[verticesStartIndex + 6] = screenX + widthScaled;
                outVertices[verticesStartIndex + 7] = screenY + 0f;
            }
            private static void PopulateTextureCoordinates(float[] outUVs, int uvsStartIndex, bool horizontalFlip, bool verticalFlip)
            {
                var minU = 0f;
                var maxU = 1f;
                if (horizontalFlip)
                {
                    minU = 1f;
                    maxU = 0f;
                }

                var minV = 0f;
                var maxV = 1f;
                if (verticalFlip)
                {
                    minV = 1f;
                    maxV = 0f;
                }

                outUVs[uvsStartIndex] = minU;
                outUVs[uvsStartIndex + 1] = minV;
                outUVs[uvsStartIndex + 2] = minU;
                outUVs[uvsStartIndex + 3] = maxV;
                outUVs[uvsStartIndex + 4] = maxU;
                outUVs[uvsStartIndex + 5] = maxV;
                outUVs[uvsStartIndex + 6] = maxU;
                outUVs[uvsStartIndex + 7] = minV;
            }
        }


        private static readonly Dictionary<string, Sprite> SpriteNames = new();
        private static readonly List<Func<Sprite>> DeferredInitialization = new();

        public static Sprite Create(string name) => new SpriteFromTexture(name, new Texture(GraphicsContextManager.Instance.GetTexture(name)));
        public static void Register(Func<Sprite> func) => DeferredInitialization.Add(func);
        public static void CreateAndRegister(string name) => Register(() => Create(name));

        internal static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((SpriteData sd) => sd.GetSprite(null!)),
                prefix: AccessTools.DeclaredMethod(typeof(SpriteDataManager), nameof(GetSpritePrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((SpriteData sd) => sd.Load(null!)),
                postfix: AccessTools.DeclaredMethod(typeof(SpriteDataManager), nameof(LoadPostfix)));
            if (!res2) return false;

            // Preventing inlining GetSprite
            harmony.TryPatch(
                AccessTools.Method(typeof(BrushFactory), "LoadBrushAnimationFrom"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(BrushFactory), "LoadBrushLayerInto"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(CanvasImage), "LoadFrom"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(CanvasLineImage), "LoadFrom"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(EditableTextWidget), "OnRender"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(ConstantDefinition), "GetValue"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(WidgetExtensions), "ConvertObject"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(WidgetExtensions), "SetWidgetAttributeFromString"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Constructor(typeof(Font), new[] { typeof(string), typeof(string), typeof(SpriteData) }),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(RichText), "FillPartsWithTokens"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            // Preventing inlining GetSprite
            // Preventing inlining Load
            harmony.TryPatch(
                AccessTools.Method(typeof(UIResourceManager), "Initialize"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            harmony.TryPatch(
                AccessTools.Method(typeof(UIContext), "Initialize"),
                transpiler: AccessTools.Method(typeof(SpriteDataManager), nameof(BlankTranspiler)));
            // Preventing inlining Load

            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetSpritePrefix(string name, ref Sprite __result)
        {
            if (!SpriteNames.TryGetValue(name, out __result))
                return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadPostfix()
        {
            foreach (var func in DeferredInitialization)
            {
                var sprite = func();
                SpriteNames[sprite.Name] = sprite;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}