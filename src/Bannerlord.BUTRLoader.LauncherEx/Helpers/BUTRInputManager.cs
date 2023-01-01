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

            var previousPressedKeys = _previousState.GetPressedKeys();
            var currentPressedKeys = _currentState.GetPressedKeys();
            var releasedKeys = previousPressedKeys.Except(currentPressedKeys).ToArray();
            ReleasedChars = releasedKeys.Select(x => _currentState.AsString(x)).Where(x => !string.IsNullOrEmpty(x)).Select(x => x[0]).ToArray();
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