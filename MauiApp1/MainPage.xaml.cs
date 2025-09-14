using MauiApp1.Platforms.Windows;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Services.Maps;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
public static class MimeTypeMapper
{
    private static readonly IDictionary<string, string> _mappings =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            /* DOCUMENTS */
            { ".pdf", "application/pdf" },
            { ".md", "text/markdown" },
            { "txt", "text/plain" },

            /* JPEG */
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},

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
        public int UploadFilesCount { get; set; }

        private readonly IFolderPicker _folderPicker;

        private readonly IApiService _apiService;

        private readonly AppShellViewModel _appShellViewModel;
        public MainPage(IFolderPicker folderPicker, IApiService apiService, AppShellViewModel appShellViewModel)
        {
            _folderPicker = folderPicker;
            _apiService = apiService;
            _appShellViewModel = appShellViewModel;
            UploadFolders = new ObservableCollection<UploadFolder> { };
            UploadFiles = new ObservableCollection<UploadFile> { };
            Folders = new ObservableCollection<Folder> { };
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
            Window secondWindow = new Window(new DisplayItemPage(_apiService, _appShellViewModel, folder));
            App.Current.OpenWindow(secondWindow);
        }
        public async void getAllUploads()
        {
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
        }
        public int getUploadFilesCount()
        {
            return UploadFiles.Count();
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
        public async void SendFiles()
        {
            int filesCount = 0;

            int totalFilesCount = UploadFiles.Count();

            foreach(UploadFile uploadFile in UploadFiles)
            {
                filesCount++;

                byte[] body = JsonSerializer.SerializeToUtf8Bytes(uploadFile);

                byte[] compressedBody = Compress(body);

                var response = await _apiService.CreatePostAsync(compressedBody);

                double progress = ((double)filesCount / (double)totalFilesCount);

                progressBarText.Text = Convert.ToString((progress * 100)) + "%";

                await progressBar.ProgressTo(value: progress, length: 900, easing: Easing.Linear);
            }
        }
        private async void OnSendDataClicked(object sender, EventArgs e)
        {
            SendFiles();
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
                string _fileName = Path.GetFileName(filePath);
                string fileExt = Path.GetExtension(filePath);
                string mimeType = MimeTypeMapper.GetMimeType(fileExt);
                if (mimeType != "application/octet-stream")
                {
                    byte[] rawData = File.ReadAllBytes(filePath);
                    string encoded = Convert.ToBase64String(rawData);
                    DateTime createdAt = File.GetCreationTime(filePath);
                    DateTime updatedAt = File.GetLastAccessTime(filePath);

                    UploadFilesCount++;

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
                UploadFolder folder = new UploadFolder { Path = folder_Path, Type = "folder" };

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

                        UploadFilesCount++;

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
                    UploadFolder subfolder = new UploadFolder { Path = subfolderPath, Type = "subfolder" };

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

                            UploadFilesCount++;

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

            labelFilesCount.Text = Convert.ToString(UploadFilesCount) + " items selected";

        }
    }
}
