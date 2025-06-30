using System.Windows;

namespace ApplicationControl
{
    public partial class BlockPopup : Window
    {
        public BlockPopup(string appName)
        {
            InitializeComponent();
            MessageText.Text = $"The application \"{appName}\" has been blocked.";
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
