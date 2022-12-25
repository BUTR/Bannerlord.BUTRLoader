using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class Keyboard
    {
        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        private static readonly byte[] _dfinedKeyCodes;
        private static readonly byte[] _keyState = new byte[256];
        private static readonly List<Keys> _keys = new(10);
        private static readonly Predicate<Keys> IsKeyReleasedPredicate = key => IsKeyReleased((byte)key);

        static Keyboard()
        {
            var definedKeys = (Keys[]) Enum.GetValues(typeof(Keys));
            _dfinedKeyCodes = definedKeys.Cast<int>().Where(keyCode => keyCode is >= 1 and <= 255).Select(keyCode => (byte) keyCode).ToArray();
        }

        public static KeyboardState GetState()
        {
            if (!GetKeyboardState(_keyState))
                return new KeyboardState(_keys, Console.CapsLock, Console.NumberLock);

            _keys.RemoveAll(IsKeyReleasedPredicate);
            foreach (var keyCode in _dfinedKeyCodes)
            {
                if (IsKeyReleased(keyCode)) continue;
                var key = (Keys)keyCode;
                if (!_keys.Contains(key))
                    _keys.Add(key);
            }

            return new KeyboardState(_keys, Console.CapsLock, Console.NumberLock);
        }

        private static bool IsKeyReleased(byte keyCode) => (_keyState[keyCode] & 0x80) == 0;
    }
}