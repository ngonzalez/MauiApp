using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

public static class MimeTypeMapper
{
    private static readonly IDictionary<string, string> _mappings =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {".png", "image/png"},
        };

    public static string GetMimeType(string extension)
    {
        if (extension == null)
        {
            throw new ArgumentNullException("extension");
        }
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }
        string mime;
        return _mappings.TryGetValue(extension, out mime) ? mime : "application/octet-stream";
    }
}

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        public class Folder
        {
            public required string Path { get; set; }
        }

        public class File
        {
            public required string Name { get; set; }
            public required string Path { get; set; }
            public required string MimeType { get; set; }
        }
        public ObservableCollection<Folder> Folders { get; set; }
        public ObservableCollection<File> Files { get; set; }

        private readonly IFolderPicker _folderPicker;

        public MainPage(IFolderPicker folderPicker)
        {
            InitializeComponent();
            _folderPicker = folderPicker;
            Folders = new ObservableCollection<Folder>{ };
            Files = new ObservableCollection<File> { };
            BindingContext = this;
        }
        private async void OnPickFolderClicked(object sender, EventArgs e)
        {
            var folderPath = await _folderPicker.PickFolder();

            FolderLabel.Text = folderPath;

            while (Folders.Count() > 0)
            {
                Folders.RemoveAt(0);
            }

            Folder folder = new Folder { Path = folderPath };

            Folders.Add(folder);

            var files = Directory.EnumerateFiles(folder.Path);

            while (Files.Count() > 0)
            {
                Folders.RemoveAt(0);
            }

            foreach (string filePath in files)
            {

                string fileName = Path.GetFileName(filePath);
                string fileExt = Path.GetExtension(filePath);
                string mimeType = MimeTypeMapper.GetMimeType(fileExt);

                Files.Add(
                    new File {
                        Name = fileName,
                        Path = filePath,
                        MimeType = mimeType,
                    }
                );
            }
        }
    }
}
