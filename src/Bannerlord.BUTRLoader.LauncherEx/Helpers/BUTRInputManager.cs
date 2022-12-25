using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    // Can't believe I need to add a custom keyboard handler for the launcher
    internal class BUTRInputManager : IInputManager
    {
        private readonly IInputManager _inputManager;

        public char[] ReleasedChars;
        private KeyboardState _currentState;
        private KeyboardState _previousState;

        public BUTRInputManager(IInputManager inputManager) => _inputManager = inputManager;

        public void Update()
        {
            _previousState = _currentState;
            _currentState = Keyboard.GetState();

            var modifiers = GetModifiers();
            var previousPressedKeys = _previousState.GetPressedKeys();
            var currentPressedKeys = _currentState.GetPressedKeys();
            var releasedKeys = previousPressedKeys.Except(currentPressedKeys).ToArray();
            ReleasedChars = releasedKeys.Select(x => ToChar(x, modifiers)).Where(x => x != char.MinValue).ToArray();
        }

        public KeyboardModifiers GetModifiers()
        {
            var modifiers = KeyboardModifiers.None;

            if (_currentState.IsKeyDown(Keys.LeftControl) || _currentState.IsKeyDown(Keys.RightControl))
                modifiers |= KeyboardModifiers.Control;

            if (_currentState.IsKeyDown(Keys.LeftShift) || _currentState.IsKeyDown(Keys.RightShift))
                modifiers |= KeyboardModifiers.Shift;

            if (_currentState.IsKeyDown(Keys.LeftAlt) || _currentState.IsKeyDown(Keys.RightAlt))
                modifiers |= KeyboardModifiers.Alt;

            return modifiers;
        }

        public bool IsMouseActive() => _inputManager.IsMouseActive();
        public float GetMousePositionX() => _inputManager.GetMousePositionX();
        public float GetMousePositionY() => _inputManager.GetMousePositionY();
        public float GetMouseScrollValue() => _inputManager.GetMouseScrollValue();
        public float GetMouseMoveY() => _inputManager.GetMouseMoveY();
        public float GetMouseSensitivity() => _inputManager.GetMouseSensitivity();
        public float GetMouseDeltaZ() => _inputManager.GetMouseDeltaZ();

        public void SetClipboardText(string text)
        {
            var thread = new Thread(() => Clipboard.SetText(text));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            //_inputManagerImplementation.SetClipboardText(text);
        }
        public string GetClipboardText()
        {
            var text = string.Empty;
            var thread = new Thread(() => text = Clipboard.GetText());
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return text;
            //return _inputManagerImplementation.GetClipboardText();
        }

        public Vec2 GetKeyState(InputKey key) =>
            IsAction(key, rawKey => new Vec2(_currentState.IsKeyDown(rawKey) ? 1f : 0f, _previousState.IsKeyDown(rawKey) ? 1f : 0f), _key => _inputManager.GetKeyState(_key));
        public bool IsKeyPressed(InputKey key) =>
            IsAction(key, rawKey => _currentState.IsKeyDown(rawKey) && _previousState.IsKeyUp(rawKey), _key => _inputManager.IsKeyPressed(_key));
        public bool IsKeyDown(InputKey key) =>
            IsAction(key, rawKey => _currentState.IsKeyDown(rawKey) || _previousState.IsKeyDown(rawKey), _key => _inputManager.IsKeyDown(_key));
        public bool IsKeyReleased(InputKey key) =>
            IsAction(key, rawKey => _currentState.IsKeyUp(rawKey) && _previousState.IsKeyDown(rawKey), _key => _inputManager.IsKeyReleased(_key));
        public bool IsKeyDownImmediate(InputKey key) =>
            IsAction(key, rawKey => _currentState.IsKeyDown(rawKey), _key => _inputManager.IsKeyDownImmediate(_key));

        public Vec2 GetResolution() => _inputManager.GetResolution();
        public Vec2 GetDesktopResolution() => _inputManager.GetDesktopResolution();

        public void SetCursorPosition(int x, int y) => _inputManager.SetCursorPosition(x, y);
        public void SetCursorFriction(float frictionValue) => _inputManager.SetCursorFriction(frictionValue);

        public void PressKey(InputKey key) => _inputManager.PressKey(key);
        public void ClearKeys() => _inputManager.ClearKeys();
        public int GetVirtualKeyCode(InputKey key) => _inputManager.GetVirtualKeyCode(key);
        public float GetMouseMoveX() => _inputManager.GetMouseMoveX();
        public void UpdateKeyData(byte[] keyData) => _inputManager.UpdateKeyData(keyData);

        public bool IsControllerConnected() => _inputManager.IsControllerConnected();
        public InputKey GetControllerClickKey() => _inputManager.GetControllerClickKey();

        private static char ToChar(Keys key, KeyboardModifiers modifiers = KeyboardModifiers.None)
        {
            var isControlDown = (modifiers & KeyboardModifiers.Control) == KeyboardModifiers.Control;
            if (isControlDown) return char.MinValue;

            var isShiftDown = (modifiers & KeyboardModifiers.Shift) == KeyboardModifiers.Shift;

            if (key == Keys.A) return isShiftDown ? 'A' : 'a';
            if (key == Keys.B) return isShiftDown ? 'B' : 'b';
            if (key == Keys.C) return isShiftDown ? 'C' : 'c';
            if (key == Keys.D) return isShiftDown ? 'D' : 'd';
            if (key == Keys.E) return isShiftDown ? 'E' : 'e';
            if (key == Keys.F) return isShiftDown ? 'F' : 'f';
            if (key == Keys.G) return isShiftDown ? 'G' : 'g';
            if (key == Keys.H) return isShiftDown ? 'H' : 'h';
            if (key == Keys.I) return isShiftDown ? 'I' : 'i';
            if (key == Keys.J) return isShiftDown ? 'J' : 'j';
            if (key == Keys.K) return isShiftDown ? 'K' : 'k';
            if (key == Keys.L) return isShiftDown ? 'L' : 'l';
            if (key == Keys.M) return isShiftDown ? 'M' : 'm';
            if (key == Keys.N) return isShiftDown ? 'N' : 'n';
            if (key == Keys.O) return isShiftDown ? 'O' : 'o';
            if (key == Keys.P) return isShiftDown ? 'P' : 'p';
            if (key == Keys.Q) return isShiftDown ? 'Q' : 'q';
            if (key == Keys.R) return isShiftDown ? 'R' : 'r';
            if (key == Keys.S) return isShiftDown ? 'S' : 's';
            if (key == Keys.T) return isShiftDown ? 'T' : 't';
            if (key == Keys.U) return isShiftDown ? 'U' : 'u';
            if (key == Keys.V) return isShiftDown ? 'V' : 'v';
            if (key == Keys.W) return isShiftDown ? 'W' : 'w';
            if (key == Keys.X) return isShiftDown ? 'X' : 'x';
            if (key == Keys.Y) return isShiftDown ? 'Y' : 'y';
            if (key == Keys.Z) return isShiftDown ? 'Z' : 'z';

            if (key == Keys.D0 && !isShiftDown || key == Keys.NumPad0) return '0';
            if (key == Keys.D1 && !isShiftDown || key == Keys.NumPad1) return '1';
            if (key == Keys.D2 && !isShiftDown || key == Keys.NumPad2) return '2';
            if (key == Keys.D3 && !isShiftDown || key == Keys.NumPad3) return '3';
            if (key == Keys.D4 && !isShiftDown || key == Keys.NumPad4) return '4';
            if (key == Keys.D5 && !isShiftDown || key == Keys.NumPad5) return '5';
            if (key == Keys.D6 && !isShiftDown || key == Keys.NumPad6) return '6';
            if (key == Keys.D7 && !isShiftDown || key == Keys.NumPad7) return '7';
            if (key == Keys.D8 && !isShiftDown || key == Keys.NumPad8) return '8';
            if (key == Keys.D9 && !isShiftDown || key == Keys.NumPad9) return '9';

            if (key == Keys.D0 && isShiftDown) return ')';
            if (key == Keys.D1 && isShiftDown) return '!';
            if (key == Keys.D2 && isShiftDown) return '@';
            if (key == Keys.D3 && isShiftDown) return '#';
            if (key == Keys.D4 && isShiftDown) return '$';
            if (key == Keys.D5 && isShiftDown) return '%';
            if (key == Keys.D6 && isShiftDown) return '^';
            if (key == Keys.D7 && isShiftDown) return '&';
            if (key == Keys.D8 && isShiftDown) return '*';
            if (key == Keys.D9 && isShiftDown) return '(';

            if (key == Keys.Space) return ' ';
            if (key == Keys.Tab) return '\t';
            if (key == Keys.Enter) return (char)13;
            if (key == Keys.Back) return (char)8;

            if (key == Keys.Add) return '+';
            if (key == Keys.Decimal) return '.';
            if (key == Keys.Divide) return '/';
            if (key == Keys.Multiply) return '*';
            if (key == Keys.OemBackslash) return '\\';
            if (key == Keys.OemComma && !isShiftDown) return ',';
            if (key == Keys.OemComma && isShiftDown) return '<';
            if (key == Keys.OemOpenBrackets && !isShiftDown) return '[';
            if (key == Keys.OemOpenBrackets && isShiftDown) return '{';
            if (key == Keys.OemCloseBrackets && !isShiftDown) return ']';
            if (key == Keys.OemCloseBrackets && isShiftDown) return '}';
            if (key == Keys.OemPeriod && !isShiftDown) return '.';
            if (key == Keys.OemPeriod && isShiftDown) return '>';
            if (key == Keys.OemPipe && !isShiftDown) return '\\';
            if (key == Keys.OemPipe && isShiftDown) return '|';
            if (key == Keys.OemPlus && !isShiftDown) return '=';
            if (key == Keys.OemPlus && isShiftDown) return '+';
            if (key == Keys.OemMinus && !isShiftDown) return '-';
            if (key == Keys.OemMinus && isShiftDown) return '_';
            if (key == Keys.OemQuestion && !isShiftDown) return '/';
            if (key == Keys.OemQuestion && isShiftDown) return '?';
            if (key == Keys.OemQuotes && !isShiftDown) return '\'';
            if (key == Keys.OemQuotes && isShiftDown) return '"';
            if (key == Keys.OemSemicolon && !isShiftDown) return ';';
            if (key == Keys.OemSemicolon && isShiftDown) return ':';
            if (key == Keys.OemTilde && !isShiftDown) return '`';
            if (key == Keys.OemTilde && isShiftDown) return '~';
            if (key == Keys.Subtract) return '-';

            return char.MinValue;
        }

        private static TReturn IsAction<TReturn>(InputKey key, Func<Keys, TReturn> action, Func<InputKey, TReturn> fallback)
        {
            var rawKey = key switch
            {
                InputKey.BackSpace => Keys.Back,
                InputKey.Enter => Keys.Enter,
                var any => Enum.TryParse<Keys>(key.ToString(), out var keyVal) ? keyVal : Keys.None
            };
            if (key is >= InputKey.ControllerLStickUp and <= InputKey.ControllerRTrigger or InputKey.ControllerLStick or InputKey.ControllerRStick)
            {
                return fallback(key);
            }
            if (key is InputKey.LeftMouseButton or InputKey.RightMouseButton or InputKey.MiddleMouseButton)
            {
                return fallback(key);
            }
            if (rawKey == Keys.None)
            {
                Trace.TraceError($"Wrong key {key}");
                return fallback(key);
            }

            return action(rawKey);
        }
    }
}