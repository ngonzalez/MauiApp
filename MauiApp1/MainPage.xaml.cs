using CommunityToolkit.Maui.Core.Primitives;
using MauiApp1.Platforms.Windows;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO.Compression;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;

public static class MimeTypeMapper
{
    private static readonly IDictionary<string, string> _mappings =
        new Dictionary<string, string>()
        {
            /* DOCUMENTS */
            { ".pdf", "application/pdf" },
            { ".md", "text/markdown" },
            { ".txt", "text/plain" },

            /* IMAGES */
            { ".bmp", "image/bmp" },
            { ".gif", "image/gif" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".tif", "image/tiff" },
            { ".tiff", "image/tiff" },
            { ".webp", "image/webp" },

            /* AUDIO */
            { ".aac", "audio/aac" },
            { ".m4a", "audio/aac" },
            { ".aff", "audio/x-aiff" },
            { ".aif", "audio/x-aiff" },
            { ".aiff", "audio/x-aiff" },
            { ".flac", "audio/flac" },
            { ".mka", "audio/x-matroska" },
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".weba", "audio/webm" },

            /* VIDEO */
            { "3gp", "video/3gpp" },
            { "mkv", "video/x-matroska" },
            { "mp4", "video/mp4" },
            { "mp4v", "video/mp4" },
            { "mpg4", "video/mp4" },
            { "m1v", "video/mpeg" },
            { "m2v", "video/mpeg" },
            { "mpg", "video/mpeg" },
            { "mpeg", "video/mpeg" },
            { "webm", "video/webm" },
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

        public int UploadFilesCount { get; set; }

        private readonly IFolderPicker _folderPicker;

        private readonly IApiService _apiService;

        private readonly IAlertService _alertService;

        private readonly AppShellViewModel _appShellViewModel;

        public MainPage(IFolderPicker folderPicker, IApiService apiService, IAlertService alertService, AppShellViewModel appShellViewModel)
        {
            _folderPicker = folderPicker;
            _apiService = apiService;
            _appShellViewModel = appShellViewModel;
            _alertService = alertService;

            var sessionID = _appShellViewModel.SessionID;

            if (sessionID == null || sessionID == 0)
            {
                Shell.Current.GoToAsync("signinpage");
            }

            UploadFolders = new ObservableCollection<UploadFolder> { };
            UploadFiles = new ObservableCollection<UploadFile> { };

            InitializeComponent();

            BindingContext = this;

            myAccountLink.Clicked += new EventHandler(accountLinkClicked);
            resetLink.Clicked += new EventHandler(resetLinkClicked);

            labelFilesCount.Text = "No items found";
            labelFilesCount.TextColor = Colors.Grey;
        }

        public int getUploadFilesCount()
        {
            return UploadFiles.Count();
        }

        public void accountLinkClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("accountpage");
        }

        public void resetLinkClicked(object sender, EventArgs e)
        {
            while (UploadFolders.Count() > 0)
            {
                UploadFolders.RemoveAt(0);
            }

            while (UploadFiles.Count() > 0)
            {
                UploadFiles.RemoveAt(0);
            }

            UploadFilesCount = UploadFiles.Count();

            // Labels
            FolderLabel.Text = "";
            labelFilesCount.Text = "No items found";
            labelFilesCount.TextColor = Colors.Grey;
            resetLink.TextColor = Colors.Grey;
            resetLinkImage.Color = Colors.Grey;

            // Progress bar
            progressBarText.Text = "";
            progressBar.ProgressTo(value: 0, length: 100, easing: Easing.Linear);
        }

        public static byte[] CompressGzip(byte[] raw)
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

        public void CompressZip(string filePath, string zipPath)
        {
            using (FileStream inputFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 1024 * 1024))
            using (FileStream targetStream = new FileStream(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(targetStream, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(filePath));

                using (Stream entryStream = entry.Open())
                {
                    inputFile.CopyTo(entryStream);
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

            foreach (UploadFile uploadFile in UploadFiles)
            {
                long length = new System.IO.FileInfo(uploadFile.filePath).Length;

                if (length >= 104857600) // 100 Megabytes = 104857600 Bytes
                {
                    string tempDirectory = GetTemporaryDirectory();
                    string fileName = Convert.ToString(uploadFile.uuid) + ".zip";
                    string tempFile = Path.Combine(tempDirectory, fileName);

                    CompressZip(uploadFile.filePath, tempFile);

                    SplitFile(tempFile, 104857600, tempDirectory);

                    try
                    {
                        System.IO.File.Delete(tempFile);
                    }
                    catch
                    {
                        //
                    }

                    var splitFiles = System.IO.Directory.GetFiles(tempDirectory);
                    int splitFilesCount = splitFiles.Count();

                    int i = 0;

                    foreach (string filePath in splitFiles)
                    {
                        SendFileBatch(uploadFile, splitFilesCount, filePath, i);

                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch
                        {
                            //
                        }

                        i++;
                    }

                    SendUploadFile(uploadFile);

                    System.Threading.Thread.Sleep(5000);
                }
                else
                {
                    SendFile(uploadFile);
                }

                // Progress bar
                filesCount++;
                double progress = ((double)filesCount / (double)totalFilesCount);
                progressBarText.Text = Convert.ToString(Convert.ToInt32(progress * 100)) + "%";
                await progressBar.ProgressTo(value: progress, length: 900, easing: Easing.Linear);
            }
        }

        private async void OnSendDataClicked(object sender, EventArgs e)
        {
            SendFiles();
        }

        public async void SendUploadFile(UploadFile uploadFile)
        {
            byte[] body = JsonSerializer.SerializeToUtf8Bytes(uploadFile);
            (int _statusCode, var response) = await _apiService.CreatePostAsync(CompressGzip(body));
        }

        public async void SendFile(UploadFile uploadFile)
        {
            using (FileStream inputFile = new FileStream(uploadFile.filePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 1024 * 1024))
            using (CryptoStream base64Stream = new CryptoStream(inputFile, new ToBase64Transform(), CryptoStreamMode.Read))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                base64Stream.CopyTo(memoryStream);
                byte[] byteArray = memoryStream.ToArray();
                memoryStream.Close();

                uploadFile.itemData = System.Text.Encoding.UTF8.GetString(byteArray);

                SendUploadFile(uploadFile);
            }
        }

        public async void SendFileBatch(UploadFile uploadFile, int filesCount, string filePath, int i)
        {
            using (FileStream inputFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 1024 * 1024))
            using (CryptoStream base64Stream = new CryptoStream(inputFile, new ToBase64Transform(), CryptoStreamMode.Read))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                base64Stream.CopyTo(memoryStream);
                byte[] byteArray = memoryStream.ToArray();
                memoryStream.Close();

                UploadFile splitUploadFile = new UploadFile
                {
                    sessionId = _appShellViewModel.SessionID,
                    uuid = Guid.NewGuid(),
                    uploadFileUuid = uploadFile.uuid,
                    itemData = System.Text.Encoding.UTF8.GetString(byteArray),
                    filePath = uploadFile.filePath + "." + (Convert.ToString(i + 1)) + "-" + filesCount + ".block",
                    mimeType = "application/octet-stream",
                    createdAt = uploadFile.createdAt,
                    updatedAt = uploadFile.updatedAt,
                    source = uploadFile.source,
                };

                SendUploadFile(splitUploadFile);
            }
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
            byte[] byteArray = new byte[4096];
            string itemData = System.Text.Encoding.UTF8.GetString(byteArray);

            if (mimeType != "application/octet-stream")
            {
                UploadFile uploadFile = new UploadFile
                {
                    sessionId = _appShellViewModel.SessionID,
                    uuid = Guid.NewGuid(),
                    createdAt = createdAt,
                    updatedAt = updatedAt,
                    filePath = filePath,
                    itemData = itemData,
                    mimeType = mimeType,
                    source = uploadFolder.Type,
                };

                UploadFiles.Add(uploadFile);

                UploadFilesCount++;

                labelFilesCount.Text = Convert.ToString(UploadFilesCount) + " " + (UploadFilesCount > 1 ? "Files" : "File") + " selected";
                labelFilesCount.TextColor = Colors.White;

                resetLink.TextColor = UploadFilesCount > 0 ? Colors.FloralWhite : Colors.Grey;
                resetLinkImage.Color = UploadFilesCount > 0 ? Colors.FloralWhite : Colors.Grey;
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
