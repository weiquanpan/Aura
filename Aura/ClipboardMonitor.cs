using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Aura
{
    public class ClipboardMonitor : IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private HwndSource _hwndSource;
        private bool _isDisposed;

        public event Action<BitmapSource> ImageCaptured;

        public ClipboardMonitor(Window window)
        {
            var helper = new WindowInteropHelper(window);
            var hwnd = helper.EnsureHandle();

            _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);

            AddClipboardFormatListener(hwnd);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    if (image != null)
                    {
                        ImageCaptured?.Invoke(image);
                    }
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (_hwndSource != null && !_hwndSource.IsDisposed)
                {
                    RemoveClipboardFormatListener(_hwndSource.Handle);
                    _hwndSource.RemoveHook(WndProc);
                    _hwndSource.Dispose();
                }
                _isDisposed = true;
            }
        }
    }
}
