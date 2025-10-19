using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aura
{
    public class TaskbarIcon : IDisposable
    {
        private const int WM_TRAYICON = 0x8001;
        private const int NIM_ADD = 0x00000000;
        private const int NIM_MODIFY = 0x00000001;
        private const int NIM_DELETE = 0x00000002;
        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(int dwMessage, [In] ref NOTIFYICONDATA lpdata);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private NOTIFYICONDATA _notifyIconData;
        private bool _isDisposed;
        private HwndSource _hwndSource;

        public event Action LeftMouseClick;
        public System.Windows.Controls.ContextMenu ContextMenu { get; set; }

        public TaskbarIcon(Window messageWindow)
        {
            _notifyIconData = new NOTIFYICONDATA();
            _notifyIconData.cbSize = Marshal.SizeOf(_notifyIconData);
            _notifyIconData.uID = 1;
            _notifyIconData.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
            _notifyIconData.uCallbackMessage = WM_TRAYICON;
            _notifyIconData.szTip = "Aura";

            var helper = new WindowInteropHelper(messageWindow);
            var hwnd = helper.EnsureHandle();
            _notifyIconData.hWnd = hwnd;

            _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);

            // Set default icon
            SetIcon(SystemIcons.Application.ToBitmap());

            Shell_NotifyIcon(NIM_ADD, ref _notifyIconData);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_TRAYICON)
            {
                switch ((int)lParam)
                {
                    case 0x202: // WM_LBUTTONUP
                        LeftMouseClick?.Invoke();
                        handled = true;
                        break;
                    case 0x205: // WM_RBUTTONUP
                        if (ContextMenu != null)
                        {
                            ContextMenu.IsOpen = true;
                            // To make the menu close when clicking away, we need to activate its window.
                            var presentationSource = PresentationSource.FromVisual(ContextMenu) as HwndSource;
                            if (presentationSource != null)
                            {
                                SetForegroundWindow(presentationSource.Handle);
                            }
                        }
                        handled = true;
                        break;
                }
            }
            return IntPtr.Zero;
        }

        public void SetIcon(Bitmap icon)
        {
            var hIcon = icon.GetHicon();
            _notifyIconData.hIcon = hIcon;
            Shell_NotifyIcon(NIM_MODIFY, ref _notifyIconData);
            DestroyIcon(hIcon);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Shell_NotifyIcon(NIM_DELETE, ref _notifyIconData);
                _hwndSource?.RemoveHook(WndProc);
                _hwndSource?.Dispose();
                _isDisposed = true;
            }
        }
    }
}
