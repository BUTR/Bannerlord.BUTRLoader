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
    internal class BUTRInputManager : IInputManager, IDisposable
    {
        public readonly IInputManager InputManager;
        public readonly int[] ReleasedChars = new int[10];

        private KeyboardState _currentState;
        private KeyboardState _previousState;

        public BUTRInputManager(IInputManager inputManager) => InputManager = inputManager;

        public void Update()
        {
            _currentState.Dispose();

            _previousState = _currentState;
            _currentState = Keyboard.GetState();

            var previousPressedKeys = _previousState.GetPressedKeys();
            var currentPressedKeys = _currentState.GetPressedKeys();

            var i = 0;
            foreach (var str in previousPressedKeys.Except(currentPressedKeys).Select(x => _currentState.AsString(x)))
            {
                if (string.IsNullOrEmpty(str)) continue;

                ReleasedChars[i] = str[0];
                i++;
            }
            for (; i < ReleasedChars.Length; i++)
            {
                ReleasedChars[i] = default;
            }
        }

        public bool IsMouseActive() => InputManager.IsMouseActive();
        public float GetMousePositionX() => InputManager.GetMousePositionX();
        public float GetMousePositionY() => InputManager.GetMousePositionY();
        public float GetMouseScrollValue() => InputManager.GetMouseScrollValue();
        public float GetMouseMoveY() => InputManager.GetMouseMoveY();
        public float GetMouseSensitivity() => InputManager.GetMouseSensitivity();
        public float GetMouseDeltaZ() => InputManager.GetMouseDeltaZ();

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
            IsAction(key, rawKey => new Vec2(_currentState.IsKeyDown(rawKey) ? 1f : 0f, _previousState.IsKeyDown(rawKey) ? 1f : 0f), _key => InputManager.GetKeyState(_key));
        public bool IsKeyPressed(InputKey key) =>
            IsAction(key, rawKey => _currentState.IsKeyDown(rawKey) && _previousState.IsKeyUp(rawKey), _key => InputManager.IsKeyPressed(_key));
        public bool IsKeyDown(InputKey key) =>
            IsAction(key, rawKey => _currentState.IsKeyDown(rawKey) || _previousState.IsKeyDown(rawKey), _key => InputManager.IsKeyDown(_key));
        public bool IsKeyReleased(InputKey key) =>
            IsAction(key, rawKey => _currentState.IsKeyUp(rawKey) && _previousState.IsKeyDown(rawKey), _key => InputManager.IsKeyReleased(_key));
        public bool IsKeyDownImmediate(InputKey key) =>
            IsAction(key, rawKey => _currentState.IsKeyDown(rawKey), _key => InputManager.IsKeyDownImmediate(_key));

        public Vec2 GetResolution() => InputManager.GetResolution();
        public Vec2 GetDesktopResolution() => InputManager.GetDesktopResolution();

        public void SetCursorPosition(int x, int y) => InputManager.SetCursorPosition(x, y);
        public void SetCursorFriction(float frictionValue) => InputManager.SetCursorFriction(frictionValue);

        public void PressKey(InputKey key) => InputManager.PressKey(key);
        public void ClearKeys() => InputManager.ClearKeys();
        public int GetVirtualKeyCode(InputKey key) => InputManager.GetVirtualKeyCode(key);
        public float GetMouseMoveX() => InputManager.GetMouseMoveX();
        public void UpdateKeyData(byte[] keyData) => InputManager.UpdateKeyData(keyData);

        public bool IsControllerConnected() => InputManager.IsControllerConnected();
        public InputKey GetControllerClickKey() => InputManager.GetControllerClickKey();

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

        public void Dispose()
        {
            _currentState.Dispose();
            _previousState.Dispose();
        }
    }
}