using System.Windows;

namespace Aura
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Settings _settings;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            _settings = SettingsManager.Load();
            ApiKeyTextBox.Text = _settings.ApiKey;
            PromptTextBox.Text = _settings.CustomPrompt;
            ProxyCheckBox.IsChecked = _settings.ProxyEnabled;
            ProxyTextBox.Text = _settings.ProxyAddress;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.ApiKey = ApiKeyTextBox.Text;
            _settings.CustomPrompt = PromptTextBox.Text;
            _settings.ProxyEnabled = ProxyCheckBox.IsChecked ?? false;
            _settings.ProxyAddress = ProxyTextBox.Text;

            SettingsManager.Save(_settings);
            this.Close();
        }
    }
}
