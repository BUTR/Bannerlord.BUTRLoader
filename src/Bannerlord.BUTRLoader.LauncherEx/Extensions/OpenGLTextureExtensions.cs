using HarmonyLib.BUTR.Extensions;

using StbSharp;

using System.IO;
using System.Runtime.InteropServices;

using TaleWorlds.TwoDimension.Standalone;

namespace Bannerlord.BUTRLoader.Extensions
{
    public static class OpenGLTextureExtensions
    {
        private enum PixelFormat : uint
        {
            ColorIndex = 6400U,
            StencilIndex,
            DepthComponent,
            Red,
            Green,
            Blue,
            Alpha,
            RGB,
            RGBA,
            Luminance,
            LuminanceAlpha,
            BGR = 32992U,
            BGRA
        }

        private delegate void MakeActiveDelegate(OpenGLTexture texture);
        private static readonly MakeActiveDelegate? _makeActiveDelegate =
            AccessTools2.GetDelegate<MakeActiveDelegate>(typeof(OpenGLTexture), "MakeActive");

        [DllImport("Opengl32.dll", EntryPoint = "glTexImage2D")]
        private static extern void TexImage2D(uint target, int level, uint internalformat, int width, int height, int border, PixelFormat format, uint type, byte[] pixels);

        public static void MakeActive(this OpenGLTexture texture) => _makeActiveDelegate?.Invoke(texture);

        public static bool LoadFromStream(this OpenGLTexture texture, string name, Stream stream)
        {
            if (_makeActiveDelegate is null)
                return false;

            var image = new ImageReader().Read(stream, 0);
            texture.Initialize(name, image.Width, image.Height);
            texture.MakeActive();
            var pixelFormat = PixelFormat.Red;
            var num = 0U;
            var flag = true;
            switch (image.Comp)
            {
                case 1:
                    pixelFormat = PixelFormat.Red;
                    num = 33321U;
                    goto IL_112;
                case 3:
                    pixelFormat = PixelFormat.RGB;
                    num = 32849U;
                    goto IL_112;
                case 4:
                    pixelFormat = PixelFormat.RGBA;
                    num = 32856U;
                    goto IL_112;
            }
            flag = false;
        IL_112:
            if (flag)
            {
                TexImage2D(0x00000DE1, 0, num, image.Width, image.Height, 0, pixelFormat, 0x00001401, image.Data);
            }

            return true;
        }
    }
}