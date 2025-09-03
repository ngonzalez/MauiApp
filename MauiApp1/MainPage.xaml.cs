using MauiApp1.Platforms.Windows;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Services.Maps;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
public static class MimeTypeMapper
{
    private static readonly IDictionary<string, string> _mappings =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
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
            _appShellViewModel = appShellViewModel;
            Folders = new ObservableCollection<Folder> { };
            UploadFiles = new ObservableCollection<UploadFile> { };
            InitializeComponent();
            BindingContext = this;
        }
        public async void SendFiles()
        {
            UploadFile uploadFile = UploadFiles[0];
            byte[] body = JsonSerializer.SerializeToUtf8Bytes(uploadFile);
            var response = await _apiService.CreatePostAsync(body);
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
                if (mimeType != "application/octet-stream")
                {
                    byte[] rawData = File.ReadAllBytes(filePath);
                    string encoded = Convert.ToBase64String(rawData);
                    DateTime createdAt = File.GetCreationTime(filePath);
                    DateTime updatedAt = File.GetLastAccessTime(filePath);

                    UploadFiles.Add(
                        new UploadFile
                        {
                            sessionId = _appShellViewModel.SessionID,
                            uuid = Guid.NewGuid(),
                            createdAt = createdAt,
                            updatedAt = updatedAt,
                            filePath = filePath,
                            itemData = encoded,
                            mimeType = mimeType,
                            source = rootFolder.Type,
                        }
                    );
                }
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
                    if (folderFileMimeType != "application/octet-stream")
                    {
                        byte[] folderFileRawData = File.ReadAllBytes(folderFilePath);
                        string folderFileEncoded = Convert.ToBase64String(folderFileRawData);
                        DateTime folderFileCreatedAt = File.GetCreationTime(folderFilePath);
                        DateTime folderFileUpdatedAt = File.GetLastAccessTime(folderFilePath);

                        UploadFiles.Add(
                            new UploadFile
                            {
                                sessionId = _appShellViewModel.SessionID,
                                uuid = Guid.NewGuid(),
                                createdAt = folderFileCreatedAt,
                                updatedAt = folderFileUpdatedAt,
                                filePath = folderFilePath,
                                itemData = folderFileEncoded,
                                mimeType = folderFileMimeType,
                                source = folder.Type,
                            }
                        );
                    }
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
                        if (subfolderFileMimeType != "application/octet-stream")
                        {
                            byte[] subfolderFileRawData = File.ReadAllBytes(subfolderFilePath);
                            string subfolderFileEncoded = Convert.ToBase64String(subfolderFileRawData);
                            DateTime subfolderFileCreatedAt = File.GetCreationTime(subfolderFilePath);
                            DateTime subfolderFileUpdatedAt = File.GetLastAccessTime(subfolderFilePath);

                            UploadFiles.Add(
                                new UploadFile
                                {
                                    sessionId = _appShellViewModel.SessionID,
                                    uuid = Guid.NewGuid(),
                                    createdAt = subfolderFileCreatedAt,
                                    updatedAt = subfolderFileUpdatedAt,
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
}
