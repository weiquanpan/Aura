using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace Aura
{
    public partial class FloatingIcon : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public event Action IconClicked;

        public FloatingIcon()
        {
            InitializeComponent();
            this.Deactivated += (s, e) => this.Hide();
            this.MouseDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) this.DragMove(); };
        }

        public void ShowAtMouseCursor()
        {
            GetCursorPos(out var cursorPos);
            this.Left = cursorPos.X;
            this.Top = cursorPos.Y;
            this.Show();
            this.Activate();
        }

        private void IconButton_Click(object sender, RoutedEventArgs e)
        {
            IconClicked?.Invoke();
            this.Hide();
        }
    }
}
