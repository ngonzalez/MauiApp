using MauiApp1.Platforms.Windows;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Bluetooth;
using Windows.Services.Maps;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class MimeTypeMapper
{
    private static readonly IDictionary<string, string> _mappings =
        new Dictionary<string, string>()
        {
            /* DOCUMENTS */
            { ".pdf", "application/pdf" },
            { ".md", "text/markdown" },
            { ".txt", "text/plain" },

            /* JPEG */
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },

            /* FLAC */
            { ".flac", "audio/flac" },

            /* MP3 */
            { ".mp3", "audio/mpeg" },

            /* AAC MP4 ALAC  **/
            { ".aac", "audio/m4a" },
            { ".m4a", "audio/x-m4a" },
            // { "mp4", "audio/mp4" },

            /* AIFF */
            { ".aff", "audio/x-aiff" },
            { ".aif", "audio/x-aiff" },
            { ".aiff", "audio/x-aiff" },

            /* WAV */
            { ".wav", "audio/wav" },

            /* MKV */
            { ".mkv", "video/x-matroska" },

            /* MP4 */
            { ".mp4", "video/mp4" },
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
        public ObservableCollection<UploadFolder> UploadFolders { get; set; }
        public ObservableCollection<UploadFile> UploadFiles { get; set; }
        public ObservableCollection<Folder> Folders { get; set; }

        public ObservableCollection<Folder> SelectedFolders { get; set; }
        public int UploadFilesCount { get; set; }

        private readonly IFolderPicker _folderPicker;

        private readonly IApiService _apiService;

        private readonly AppShellViewModel _appShellViewModel;
        public MainPage(IFolderPicker folderPicker, IApiService apiService, AppShellViewModel appShellViewModel)
        {
            _folderPicker = folderPicker;
            _apiService = apiService;
            _appShellViewModel = appShellViewModel;

            var sessionID = _appShellViewModel.SessionID;

            if (sessionID == null || sessionID == 0)
            {
                Shell.Current.GoToAsync("signin");
            }

            UploadFolders = new ObservableCollection<UploadFolder> { };
            UploadFiles = new ObservableCollection<UploadFile> { };
            Folders = new ObservableCollection<Folder> { };
            SelectedFolders = new ObservableCollection<Folder> { };

            InitializeComponent();

            BindingContext = this;

            myAccountLink.Clicked += new EventHandler(accountLinkClicked);
            refreshFilesButton.Clicked += new EventHandler(refreshButtonClicked);

            labelFilesCount.Text = "no items found";

            getAllUploads();
        }
        public async void displayFiles(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Folder folder = (Folder)button.BindingContext;
            Window secondWindow = new Window(new DisplayPage(_apiService, _appShellViewModel, folder));
            App.Current.OpenWindow(secondWindow);
        }
        public async void getAllUploads()
        {
            ActivityIndicator.IsRunning = true;

            var response = await _apiService.GetAllUploads("");
            var uploadsResponse = JsonSerializer.Deserialize<Upload[]>(response);

            while (Folders.Count() > 0) {
                Folders.RemoveAt(0);
            }

            foreach (var item in uploadsResponse) {
                if (item.imageFiles.Length > 0)
                {
                    foreach (ImageFile imageFile in item.imageFiles)
                    {
                        if (imageFile.folder != null && imageFile.folder.name != "")
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.name == imageFile.folder.name)
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                Folders.Add(imageFile.folder);
                            }
                        }
                    }
                }
                if (item.textFiles.Length > 0)
                {
                    foreach (TextFile textFile in item.textFiles)
                    {
                        if (textFile.folder != null && textFile.folder.name != "")
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.name == textFile.folder.name)
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                Folders.Add(textFile.folder);
                            }
                        }
                    }
                }
                if (item.pdfFiles.Length > 0)
                {
                    foreach (PdfFile pdfFile in item.pdfFiles)
                    {
                        if (pdfFile.folder != null && pdfFile.folder.name != "")
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.name == pdfFile.folder.name)
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                Folders.Add(pdfFile.folder);
                            }
                        }
                    }
                }
                if (item.audioFiles.Length > 0)
                {
                    foreach (AudioFile audioFile in item.audioFiles)
                    {
                        if (audioFile.folder != null && audioFile.folder.name != "")
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.name == audioFile.folder.name)
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                Folders.Add(audioFile.folder);
                            }
                        }
                    }
                }
                if (item.videoFiles.Length > 0)
                {
                    foreach (VideoFile videoFile in item.videoFiles)
                    {
                        if (videoFile.folder != null && videoFile.folder.name != "")
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.name == videoFile.folder.name)
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                Folders.Add(videoFile.folder);
                            }
                        }
                    }
                }
            }

            ActivityIndicator.IsRunning = false;

            foldersCount.Text = Convert.ToString(Folders.Count() + " folders");
        }
        public int getUploadFilesCount()
        {
            return UploadFiles.Count();
        }
        public async void FolderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Folder selectedFolder = e.CurrentSelection.FirstOrDefault() as Folder;

            SelectedFolders.Add(selectedFolder);
        }
        public void accountLinkClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("account");
        }
        public void refreshButtonClicked(object sender, EventArgs e)
        {
            getAllUploads();
        }
        public static byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        public async void SendData(UploadFile uploadFile, string filePath, int i)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[25165824];
                    int bytesRead;

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        memoryStream.Write(buffer, 0, bytesRead);
                    }

                    byte[] byteArray = memoryStream.ToArray();

                    UploadFile newUploadFile = new UploadFile
                    {
                        sessionId = _appShellViewModel.SessionID,
                        uuid = Guid.NewGuid(),
                        uploadFileUuid = uploadFile.uuid,
                        createdAt = uploadFile.createdAt,
                        updatedAt = uploadFile.updatedAt,
                        source = uploadFile.source,
                        filePath = string.Concat(uploadFile.filePath + "." + Convert.ToString(i) + ".block"),
                        itemData = Convert.ToBase64String(byteArray),
                        mimeType = "application/octet-stream",
                    };

                    byte[] body = JsonSerializer.SerializeToUtf8Bytes(newUploadFile);
                    byte[] compressedBody = Compress(body);

                    var response = await _apiService.CreatePostAsync(compressedBody);
                }
            }
        }

        public string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            if (System.IO.Directory.Exists(tempDirectory))
            {
                return GetTemporaryDirectory();
            }
            else
            {
                Directory.CreateDirectory(tempDirectory);
                return tempDirectory;
            }
        }

        public async void SendFiles()
        {
            int filesCount = 0;
            int totalFilesCount = UploadFiles.Count();

            foreach(UploadFile uploadFile in UploadFiles)
            {
                string tempDirectory = GetTemporaryDirectory();

                SplitFile(uploadFile.filePath, 14680064, tempDirectory); // 14 Megabytes = 14680064 Bytes

                int i = 0;

                foreach(string filePath in System.IO.Directory.GetFiles(tempDirectory))
                {
                    SendData(uploadFile, filePath, i);

                    i++;
                }

                Directory.Delete(tempDirectory);

                // Progress bar
                filesCount++;
                double progress = ((double)filesCount / (double)totalFilesCount);
                progressBarText.Text = Convert.ToString((progress * 100)) + "%";
                await progressBar.ProgressTo(value: progress, length: 900, easing: Easing.Linear);
            }
        }

        private async void OnSendDataClicked(object sender, EventArgs e)
        {
            SendFiles();
        }

        public async void SplitFile(string inputFile, int chunkSize, string path)
        {
            byte[] buffer = new byte[chunkSize];

            using (Stream input = System.IO.File.OpenRead(inputFile))
            {
                int index = 0;
                while (input.Position < input.Length)
                {
                    using (Stream output = System.IO.File.Create(path + "\\" + index))
                    {
                        int chunkBytesRead = 0;
                        while (chunkBytesRead < chunkSize)
                        {
                            int bytesRead = input.Read(buffer,
                                                       chunkBytesRead,
                                                       chunkSize - chunkBytesRead);

                            if (bytesRead == 0)
                            {
                                break;
                            }
                            chunkBytesRead += bytesRead;
                        }
                        output.Write(buffer, 0, chunkBytesRead);
                    }
                    index++;
                }
            }
        }

        public async void CreateUploadFile(string filePath, UploadFolder uploadFolder)
        {
            DateTime createdAt = System.IO.File.GetCreationTime(filePath);
            DateTime updatedAt = System.IO.File.GetLastAccessTime(filePath);

            string _fileName = Path.GetFileName(filePath);
            string fileExt = Path.GetExtension(filePath);
            string mimeType = MimeTypeMapper.GetMimeType(fileExt);

            if (mimeType != "application/octet-stream")
            {
                UploadFiles.Add(
                    new UploadFile
                    {
                        sessionId = _appShellViewModel.SessionID,
                        uuid = Guid.NewGuid(),
                        createdAt = createdAt,
                        updatedAt = updatedAt,
                        filePath = filePath,
                        itemData = "",
                        mimeType = mimeType,
                        source = uploadFolder.Type,
                    }
                );

                UploadFilesCount++;

                labelFilesCount.Text = Convert.ToString(UploadFilesCount) + " items selected";
            }
        }

        private async void OnPickFolderClicked(object sender, EventArgs e)
        {
            int UploadFilesCount = 0;

            string folderPath = await _folderPicker.PickFolder();

            if (folderPath == "")
            {
                return;
            }

            UploadFolder rootFolder = new UploadFolder { Path = folderPath, Type = "root" };

            FolderLabel.Text = rootFolder.Path;

            while (UploadFolders.Count() > 0)
            {
                UploadFolders.RemoveAt(0);
            }

            UploadFolders.Add(rootFolder);

            var files = Directory.EnumerateFiles(rootFolder.Path);

            while (UploadFiles.Count() > 0)
            {
                UploadFiles.RemoveAt(0);
            }

            foreach (string filePath in files)
            {
                CreateUploadFile(filePath, rootFolder);
            }

            var folders = Directory.EnumerateDirectories(rootFolder.Path);

            foreach (string folder_Path in folders)
            {
                UploadFolder folder = new UploadFolder { Path = folder_Path, Type = "folder" };

                var folderFiles = Directory.EnumerateFiles(folder.Path);

                foreach (string folderFilePath in folderFiles)
                {
                    CreateUploadFile(folderFilePath, folder);
                }

                var folderFolders = Directory.EnumerateDirectories(folder.Path);

                foreach (string subfolderPath in folderFolders)
                {
                    UploadFolder subfolder = new UploadFolder { Path = subfolderPath, Type = "subfolder" };

                    var subfolderFiles = Directory.EnumerateFiles(subfolder.Path);

                    foreach (string subfolderFilePath in subfolderFiles)
                    {
                        CreateUploadFile(subfolderFilePath, subfolder);
                    }
                }
            }
        }
    }
}
