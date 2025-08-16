using System.Collections.ObjectModel;

namespace MauiApp1
{
    public class Folder
    {
        public required string Path { get; set; }
        public required string Type { get; set; }
    }
    public class File
    {
        public required string Name { get; set; }
        public required string Path { get; set; }
        public required string MimeType { get; set; }
        public required string Type { get; set; }
    }
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
