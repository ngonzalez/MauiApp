using MauiApp1.Platforms.Windows;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Services.Maps;
using static System.Net.Mime.MediaTypeNames;
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
        public ObservableCollection<UploadFile> UploadFiles { get; set; }

        private readonly IFolderPicker _folderPicker;

        private readonly IApiService _apiService;

        private readonly AppShellViewModel _appShellViewModel;
        public MainPage(IFolderPicker folderPicker, IApiService apiService, AppShellViewModel appShellViewModel)
        {
            _folderPicker = folderPicker;
            _apiService = apiService;
            _appShellViewModel = appShellViewModel; // _appShellViewModel.CurrentUser
            Folders = new ObservableCollection<Folder> { };
            UploadFiles = new ObservableCollection<UploadFile> { };
            InitializeComponent();
            BindingContext = this;
        }
        public async void SendFiles()
        {
            var uploadFile = UploadFiles[0];
            var json = JsonSerializer.Serialize(uploadFile);

            var _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };
            var jsonContent = new StringContent(JsonSerializer.Serialize(json, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await _apiService.CreatePostAsync(jsonContent);
            await DisplayAlert("Alert", response, "OK");
        }
        private async void OnSendDataClicked(object sender, EventArgs e)
        {
            SendFiles();
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

            while (UploadFiles.Count() > 0)
            {
                UploadFiles.RemoveAt(0);
            }

            foreach (string filePath in files)
            {

                string _fileName = Path.GetFileName(filePath);
                string fileExt = Path.GetExtension(filePath);
                string mimeType = MimeTypeMapper.GetMimeType(fileExt);
                byte[] rawData = File.ReadAllBytes(filePath);
                string encoded = Convert.ToBase64String(rawData);

                UploadFiles.Add(

                    new UploadFile
                    {
                        uuid = Guid.NewGuid(),
                        filePath = filePath,
                        itemData = encoded,
                        mimeType = mimeType,
                        source = rootFolder.Type,
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

                    string _folderFileFileName = Path.GetFileName(folderFilePath);
                    string folderFileFileExt = Path.GetExtension(folderFilePath);
                    string folderFileMimeType = MimeTypeMapper.GetMimeType(folderFileFileExt);
                    byte[] folderFileRawData = File.ReadAllBytes(folderFilePath);
                    string folderFileEncoded = Convert.ToBase64String(folderFileRawData);

                    UploadFiles.Add(
                        new UploadFile
                        {
                            uuid = Guid.NewGuid(),
                            filePath = folderFilePath,
                            itemData = folderFileEncoded,
                            mimeType = folderFileMimeType,
                            source = folder.Type,
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

                        string _subfolderFileFileName = Path.GetFileName(subfolderFilePath);
                        string subfolderFileFileExt = Path.GetExtension(subfolderFilePath);
                        string subfolderFileMimeType = MimeTypeMapper.GetMimeType(subfolderFileFileExt);
                        byte[] subfolderFileRawData = File.ReadAllBytes(subfolderFilePath);
                        string subfolderFileEncoded = Convert.ToBase64String(subfolderFileRawData);

                        UploadFiles.Add(
                            new UploadFile
                            {
                                uuid = Guid.NewGuid(),
                                filePath = subfolderFilePath,
                                itemData = subfolderFileEncoded,
                                mimeType = subfolderFileMimeType,
                                source = subfolder.Type,
                            }
                        );
                    }
                }
            }
        }
    }
}
