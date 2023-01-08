using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Bannerlord.BUTRLoader.Helpers
{
    public sealed class DragAndDropManager : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct POINT { public readonly int x, y; }
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MESSAGE
        {
            public readonly IntPtr hwnd;
            public readonly uint message;
            public readonly HDROP wParam;
            public readonly IntPtr lParam;
            public readonly ushort time;
            public readonly POINT pt;
        }

        private readonly HWND _hWindow;
        private readonly HOOKPROC _del;
        private readonly UnhookWindowsHookExSafeHandle _handle;
        private readonly Action<List<string>>? _onFileDrop;

        public DragAndDropManager(Action<List<string>>? onFileDrop)
        {
            var threadId = PInvoke.GetCurrentThreadId();
            var hModule = PInvoke.GetModuleHandle((string?) null);

            _onFileDrop = onFileDrop;
            _hWindow = PInvoke.GetForegroundWindow();
            _del = (HOOKPROC) Lpfn;
            _handle = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_GETMESSAGE, _del, hModule, threadId);
            PInvoke.DragAcceptFiles(_hWindow, true);
        }

        private unsafe LRESULT Lpfn(int code, WPARAM wPram, LPARAM lParam)
        {
            var message = Unsafe.Read<MESSAGE>((void*) lParam.Value);
            if (code == 0 && message.message == 0x233)
            {
                const int buffLen = 256;
                const int buffLen2 = 1024 * 5;
                var buff = stackalloc char[buffLen];

                var result = new List<string>();
                var count = PInvoke.DragQueryFile(message.wParam, 0xFFFFFFFF, null, 0);
                for (uint i = 0; i < count; i++)
                {
                    var len = (int) PInvoke.DragQueryFile(message.wParam, i, new PWSTR(buff), buffLen);
                    if (len < buffLen - 1)
                    {
                        result.Add(new ReadOnlySpan<char>(buff, len).ToString());
                        continue;
                    }

                    fixed (char* buff2 = new char[buffLen2])
                    {
                        var len2 = (int) PInvoke.DragQueryFile(message.wParam, i, new PWSTR(buff2), buffLen2);
                        if (len2 == buffLen2 - 1) continue;
                        result.Add(new ReadOnlySpan<char>(buff2, len2).ToString());
                    }
                }

                PInvoke.DragFinish(message.wParam);
                _onFileDrop?.Invoke(result);
            }

            return PInvoke.CallNextHookEx(HHOOK.Null, code, wPram, lParam);
        }

        public void Dispose()
        {
            _handle.Dispose();
            PInvoke.DragAcceptFiles(_hWindow, false);
        }
    }
}