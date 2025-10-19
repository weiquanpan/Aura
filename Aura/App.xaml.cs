using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Aura
{
    public partial class App : Application
    {
        public static Settings AppSettings { get; private set; }

        private TaskbarIcon _taskbarIcon;
        private ClipboardMonitor _clipboardMonitor;
        private GlobalMouseHook _mouseHook;
        private TextCapture _textCapture;
        private Window _messageWindow;
        private FloatingIcon _floatingIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppSettings = SettingsManager.Load();

            _messageWindow = new Window { Visibility = Visibility.Hidden, ShowInTaskbar = false };
            _messageWindow.Show();

            MainWindow = new MainWindow();
            MainWindow.Hide();

            _floatingIcon = new FloatingIcon();
            _floatingIcon.IconClicked += OnFloatingIconClicked;

            _taskbarIcon = new TaskbarIcon(_messageWindow);

            _clipboardMonitor = new ClipboardMonitor(_messageWindow);
            _clipboardMonitor.ImageCaptured += OnImageCaptured;

            _textCapture = new TextCapture();
            _textCapture.TextCaptured += OnTextCaptured;

            _mouseHook = new GlobalMouseHook();
            _mouseHook.LeftButtonUp += OnLeftButtonUp;

            var contextMenu = new ContextMenu();
            var settingsMenuItem = new MenuItem { Header = "Settings" };
            settingsMenuItem.Click += SettingsMenuItem_Click;
            var exitMenuItem = new MenuItem { Header = "Exit" };
            exitMenuItem.Click += (s, args) => Shutdown();

            contextMenu.Items.Add(settingsMenuItem);
            contextMenu.Items.Add(exitMenuItem);

            _taskbarIcon.ContextMenu = contextMenu;
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }

        private object _capturedContent;

        private void OnImageCaptured(BitmapSource image)
        {
            _capturedContent = image;
            _floatingIcon.ShowAtMouseCursor();
        }

        private void OnLeftButtonUp()
        {
            Task.Delay(100).ContinueWith(_ =>
                Application.Current.Dispatcher.Invoke(() =>
                    _textCapture.TryCaptureSelectedText()
                )
            );
        }

        private void OnTextCaptured(string text)
        {
            _capturedContent = text;
            _floatingIcon.ShowAtMouseCursor();
        }

        private async void OnFloatingIconClicked()
        {
            if (_capturedContent == null) return;

            // Optionally, show a "loading" indicator here.
            // For simplicity, we'll proceed directly to the API call.

            var apiService = new GeminiApiService(AppSettings);
            var result = await apiService.GenerateContentAsync(_capturedContent);

            var resultWindow = new ResultWindow();
            resultWindow.ShowResult(result);

            _capturedContent = null; // Clear content after processing
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mouseHook?.Dispose();
            _clipboardMonitor?.Dispose();
            _taskbarIcon?.Dispose();
            base.OnExit(e);
        }

        private void Shutdown()
        {
            _messageWindow?.Close();
            MainWindow?.Close();
            Current.Shutdown();
        }
    }
}
