using System.Threading;
using System.Windows;

namespace ApplicationControl
{
    public partial class MainWindow : Window
    {
        private readonly AppConfig config;
        private bool isSidebarVisible = false;
        private ProcessMonitor monitor;

        // Binding property for showing/hiding text
        public Visibility TextVisibility { get; set; } = Visibility.Collapsed;

        public MainWindow()
        {
            InitializeComponent();

            config = AppConfig.Load();
            monitor = new ProcessMonitor(config);
            monitor.Start();

            // Set DataContext for binding
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load default screen and collapse sidebar initially
            SidebarColumn.Width = new GridLength(60);
            TextVisibility = Visibility.Collapsed;
            MainFrame.Content = new Home();

            // Force rebind after setting TextVisibility
            this.DataContext = null;
            this.DataContext = this;
        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            isSidebarVisible = !isSidebarVisible;

            if (isSidebarVisible)
            {
                SidebarColumn.Width = new GridLength(190);
                TextVisibility = Visibility.Visible;
            }
            else
            {
                SidebarColumn.Width = new GridLength(60);
                TextVisibility = Visibility.Collapsed;
            }

            // Refresh binding
            this.DataContext = null;
            this.DataContext = this;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new Home();
        }

        private void ManageApps_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ManageApps(config);
        }
    }
}
