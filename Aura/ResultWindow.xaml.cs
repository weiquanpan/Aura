using System.Windows;

namespace Aura
{
    public partial class ResultWindow : Window
    {
        public ResultWindow()
        {
            InitializeComponent();
        }

        public void ShowResult(string text)
        {
            ResultTextBlock.Text = text;
            this.Show();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
