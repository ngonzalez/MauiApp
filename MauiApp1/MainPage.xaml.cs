using MauiApp1.Platforms.Windows;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Services.Maps;
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
        public ObservableCollection<Folder> Folders { get; set; }
        public ObservableCollection<File> Files { get; set; }

        private readonly IFolderPicker _folderPicker;

        private readonly IApiService _apiService;

        private readonly IAuthenticate _authenticate;
        public MainPage(IFolderPicker folderPicker, IApiService apiService, IAuthenticate authenticate)
        {
            InitializeComponent();
            _folderPicker = folderPicker;
            _apiService = apiService;
            _authenticate = authenticate;
            Folders = new ObservableCollection<Folder> { };
            Files = new ObservableCollection<File> { };
            BindingContext = this;
        }
        public async void SendFiles()
        {
//            var json = JsonSerializer.Serialize(Files);
//            var jsonContent = new StringContent(JsonSerializer.Serialize(json, _jsonOptions), Encoding.UTF8, "application/json");
//            var response = await _apiService.CreatePostAsync(jsonContent);
            await DisplayAlert("Alert", "response", "OK");
        }
        private async void OnSendDataClicked(object sender, EventArgs e)
        {
            //SendFiles();
        }
        private async void OnPickFolderClicked(object sender, EventArgs e)
        {
            var folderPath = await _folderPicker.PickFolder();

            if (folderPath == "")
            {
                return;
            }

            Folder rootFolder = new Folder { Path = folderPath, Type = "root" };

            FolderLabel.Text = rootFolder.Path;

            while (Folders.Count() > 0)
            {
                Folders.RemoveAt(0);
            }

            Folders.Add(rootFolder);

            var files = Directory.EnumerateFiles(rootFolder.Path);

            while (Files.Count() > 0)
            {
                Files.RemoveAt(0);
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
                        Type = rootFolder.Type,
                    }
                );
            }

            var folders = Directory.EnumerateDirectories(rootFolder.Path);

            foreach (string folder_Path in folders)
            {
                Folder folder = new Folder { Path = folder_Path, Type = "folder" };

                var folderFiles = Directory.EnumerateFiles(folder.Path);

                foreach (string folderFilePath in folderFiles)
                {

                    string folderFileFileName = Path.GetFileName(folderFilePath);
                    string folderFileFileExt = Path.GetExtension(folderFilePath);
                    string folderFileMimeType = MimeTypeMapper.GetMimeType(folderFileFileExt);

                    Files.Add(
                        new File
                        {
                            Name = folderFileFileName,
                            Path = folderFilePath,
                            MimeType = folderFileMimeType,
                            Type = folder.Type,
                        }
                    );
                }

                var folderFolders = Directory.EnumerateDirectories(folder.Path);

                foreach (string subfolderPath in folderFolders)
                {
                    Folder subfolder = new Folder { Path = subfolderPath, Type = "subfolder" };

                    var subfolderFiles = Directory.EnumerateFiles(subfolder.Path);

                    foreach (string subfolderFilePath in subfolderFiles)
                    {

                        string subfolderFileFileName = Path.GetFileName(subfolderFilePath);
                        string subfolderFileFileExt = Path.GetExtension(subfolderFilePath);
                        string subfolderFileMimeType = MimeTypeMapper.GetMimeType(subfolderFileFileExt);

                        Files.Add(
                            new File
                            {
                                Name = subfolderFileFileName,
                                Path = subfolderFilePath,
                                MimeType = subfolderFileMimeType,
                                Type = subfolder.Type,
                            }
                        );
                    }
                }
            }
        }
    }
}
