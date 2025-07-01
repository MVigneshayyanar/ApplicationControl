namespace ApplicationControl
{
    public class AppInfo
    {
        public string ProductName { get; set; }
        public string VendorName { get; set; }
        public string FileHash { get; set; }
        public string ExeName { get; set; }
        public string FilePath { get; set; }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(ProductName)
                ? ProductName
                : ExeName ?? "Unknown Application";
        }
    }
}
