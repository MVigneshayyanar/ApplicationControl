using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ApplicationControl
{
    public partial class ManageApps : UserControl
    {
        private AppConfig config;

        public ObservableCollection<string> Whitelist { get; set; }
        public ObservableCollection<string> Blacklist { get; set; }

        public ManageApps(AppConfig config)
        {
            InitializeComponent();
            this.config = config;

            Whitelist = new ObservableCollection<string>(config.Whitelist);
            Blacklist = new ObservableCollection<string>(config.Blacklist);

            WhitelistBox.ItemsSource = Whitelist;
            BlacklistBox.ItemsSource = Blacklist;
        }

        private void AddWhitelist(object sender, RoutedEventArgs e)
        {
            var app = WhitelistInput.Text.Trim();
            if (!string.IsNullOrEmpty(app) && !Whitelist.Contains(app))
            {
                Whitelist.Add(app);
                config.Whitelist.Add(app);
                AppConfig.Save(config);
                WhitelistInput.Clear();
            }
        }

        private void RemoveWhitelist(object sender, RoutedEventArgs e)
        {
            var selectedApp = WhitelistBox.SelectedItem as string;
            if (selectedApp != null)
            {
                Whitelist.Remove(selectedApp);
                config.Whitelist.Remove(selectedApp);
                AppConfig.Save(config);
            }
        }

        private void AddBlacklist(object sender, RoutedEventArgs e)
        {
            var app = BlacklistInput.Text.Trim();
            if (!string.IsNullOrEmpty(app) && !Blacklist.Contains(app))
            {
                Blacklist.Add(app);
                config.Blacklist.Add(app);
                AppConfig.Save(config);
                BlacklistInput.Clear();
            }
        }

        private void RemoveBlacklist(object sender, RoutedEventArgs e)
        {
            var selectedApp = BlacklistBox.SelectedItem as string;
            if (selectedApp != null)
            {
                Blacklist.Remove(selectedApp);
                config.Blacklist.Remove(selectedApp);
                AppConfig.Save(config);
            }
        }
    }
}
