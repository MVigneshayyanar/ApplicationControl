using System.Windows;

namespace ApplicationControl
{
    public partial class AppMetadataInput : Window
    {
        public string ProductName => ProductNameBox.Text.Trim();
        public string VendorName => VendorNameBox.Text.Trim();
        public string FileHash => FileHashBox.Text.Trim();
        public string ExeName => ExeNameBox.Text.Trim();

        public AppMetadataInput(string productName = "", string vendorName = "", string fileHash = "", string exeName = "")
        {
            InitializeComponent();
            ProductNameBox.Text = productName;
            VendorNameBox.Text = vendorName;
            FileHashBox.Text = fileHash;
            ExeNameBox.Text = exeName;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductNameBox.Text))
            {
                MessageBox.Show("Product name is mandatory.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
