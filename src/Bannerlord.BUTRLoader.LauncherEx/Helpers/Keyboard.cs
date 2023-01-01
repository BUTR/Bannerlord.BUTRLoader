using System;
using System.Runtime.InteropServices;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class Keyboard
    {
        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        private static readonly byte[] _keyState = new byte[256];

        public static KeyboardState GetState() => !GetKeyboardState(_keyState)
            ? new KeyboardState(Array.Empty<byte>(), Console.CapsLock, Console.NumberLock)
            : new KeyboardState(_keyState, Console.CapsLock, Console.NumberLock);
    }
}