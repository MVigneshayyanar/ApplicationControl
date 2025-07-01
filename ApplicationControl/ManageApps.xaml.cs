using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ApplicationControl
{
    public partial class ManageApps : UserControl
    {
        private AppConfig config;
        public ObservableCollection<AppInfo> Whitelist { get; set; }
        public ObservableCollection<AppInfo> Blacklist { get; set; }

        public ManageApps(AppConfig config)
        {
            InitializeComponent();
            this.config = config;

            Whitelist = new ObservableCollection<AppInfo>(config.Whitelist);
            Blacklist = new ObservableCollection<AppInfo>(config.Blacklist);

            WhitelistBox.ItemsSource = Whitelist;
            BlacklistBox.ItemsSource = Blacklist;
        }

        private void AddAppToList(ObservableCollection<AppInfo> collection, List<AppInfo> configList)
        {
            var dialog = new AppMetadataInput(); // All fields are entered manually

            if (dialog.ShowDialog() == true)
            {
                // At least one field must be filled
                if (string.IsNullOrWhiteSpace(dialog.ProductName) &&
                    string.IsNullOrWhiteSpace(dialog.VendorName) &&
                    string.IsNullOrWhiteSpace(dialog.FileHash) &&
                    string.IsNullOrWhiteSpace(dialog.ExeName))
                {
                    MessageBox.Show("You must enter at least one field: Product Name, Vendor, Hash, or EXE name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var app = new AppInfo
                {
                    ProductName = dialog.ProductName,
                    VendorName = dialog.VendorName,
                    FileHash = dialog.FileHash?.ToLowerInvariant(),
                    FilePath = "", // Optional
                    ExeName = dialog.ExeName?.ToLowerInvariant()
                };

                collection.Add(app);
                configList.Add(app);
                AppConfig.Save(config);
            }
        }

        private void AddWhitelist(object sender, RoutedEventArgs e)
        {
            AddAppToList(Whitelist, config.Whitelist);
        }

        private void AddBlacklist(object sender, RoutedEventArgs e)
        {
            AddAppToList(Blacklist, config.Blacklist);
        }

        private void RemoveWhitelist(object sender, RoutedEventArgs e)
        {
            if (WhitelistBox.SelectedItem is AppInfo selected)
            {
                Whitelist.Remove(selected);
                config.Whitelist.Remove(selected);
                AppConfig.Save(config);
            }
        }

        private void RemoveBlacklist(object sender, RoutedEventArgs e)
        {
            if (BlacklistBox.SelectedItem is AppInfo selected)
            {
                Blacklist.Remove(selected);
                config.Blacklist.Remove(selected);
                AppConfig.Save(config);
            }
        }
    }
}
