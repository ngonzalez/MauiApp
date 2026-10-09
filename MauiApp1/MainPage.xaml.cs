using CommunityToolkit.Maui.Core.Primitives;
using MauiApp1.Platforms.Windows;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO.Compression;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text.Json;
using MauiApp1.Resources.Strings;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<UploadFolder> UploadFolders { get; set; }

        public ObservableCollection<UploadFile> UploadFiles { get; set; }

        public int UploadFilesCount { get; set; }

        private readonly IFolderPicker _folderPicker;

        private readonly IApiService _apiService;

        private readonly IAuthenticate _authenticate;

        private readonly IAlertService _alertService;

        private readonly AppShellViewModel _appShellViewModel;

        public MainPage(IAuthenticate authenticate, IFolderPicker folderPicker, IApiService apiService, IAlertService alertService, AppShellViewModel appShellViewModel)
        {
            _folderPicker = folderPicker;
            _authenticate = authenticate;
            _apiService = apiService;
            _alertService = alertService;
            _appShellViewModel = appShellViewModel;

            if (string.IsNullOrEmpty(_appShellViewModel.Token))
            {
                Shell.Current.GoToAsync("signinpage");
            }

            UploadFolders = new ObservableCollection<UploadFolder> { };
            UploadFiles = new ObservableCollection<UploadFile> { };

            InitializeComponent();

            BindingContext = this;

            myAccountLink.Clicked += new EventHandler(accountLinkClicked);
            resetLink.Clicked += new EventHandler(resetLinkClicked);

            labelFilesCount.Text = AppResources.Get("NoItemsFound");
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
            labelFilesCount.Text = AppResources.Get("NoItemsFound");
            labelFilesCount.TextColor = Colors.Grey;
            resetLink.TextColor = Colors.Grey;
            resetLinkImage.Color = Colors.Grey;

            // Progress bar
            progressBarText.Text = "";
            progressBar.ProgressTo(value: 0, length: 100, easing: Easing.Linear);
        }

        // Send each file straight to the storage, in one request
        // (DirectUploadClient), one after the other; a failed file doesn't stop
        // the others and is reported at the end
        // True while SendFiles runs: a second click doesn't send the files twice
        private bool _sending;

        public async Task SendFiles()
        {
            if (_sending)
            {
                return;
            }
            _sending = true;
            try
            {
                await SendEachFile();
            }
            finally
            {
                _sending = false;
            }
        }

        private async Task SendEachFile()
        {
            int filesCount = 0;
            int totalFilesCount = UploadFiles.Count();
            var failures = new List<string>();

            foreach (UploadFile uploadFile in UploadFiles.ToList())
            {
                try
                {
                    long length = new System.IO.FileInfo(uploadFile.filePath).Length;

                    var fileInfo = new Dictionary<string, string>
                    {
                        { "fileSize", Convert.ToString(length) },
                        { "fileName", uploadFile.filePath },
                        { "mimeType", uploadFile.mimeType }
                    };

                    uploadFile.uuid = await _apiService.UploadFileAsync(uploadFile);

                    // The file is uploaded: an event that can't be recorded
                    // doesn't make it a failed upload
                    try
                    {
                        await CreateEvent(uploadFile, fileInfo);
                    }
                    catch (Exception exception)
                    {
                        CrashLog.Write(exception, "CreateEvent");
                    }
                }
                catch (Exception exception)
                {
                    failures.Add(Path.GetFileName(uploadFile.filePath) + ": " + exception.Message);
                }

                // Progress bar
                filesCount++;
                double progress = ((double)filesCount / (double)totalFilesCount);
                progressBarText.Text = Convert.ToString(Convert.ToInt32(progress * 100)) + "%";
                await progressBar.ProgressTo(value: progress, length: 900, easing: Easing.Linear);
            }

            if (failures.Count > 0)
            {
                await _alertService.DisplayAlertAsync(AppResources.Count("UploadsFailed", failures.Count), string.Join("\n", failures), "OK");
            }
        }

        private async Task CreateEvent(UploadFile uploadFile, Dictionary<string, string> uploadDetails)
        {
            var accountUuid = _appShellViewModel.CurrentUser.accountUuid;
            var accountUuidUnwrapped = accountUuid!;
            var url = "https://appshare.site/uploads/" + Convert.ToString(uploadFile.uuid);

            var platformDetails = new Dictionary<string, string> {
                { "Model", DeviceInfo.Current.Model },
                { "Manufacturer", DeviceInfo.Current.Manufacturer },
                { "Name", DeviceInfo.Current.Name },
                { "VersionString", DeviceInfo.Current.VersionString },
                { "Idiom", Convert.ToString(DeviceInfo.Current.Idiom) },
                { "Platform", Convert.ToString(DeviceInfo.Current.Platform) },
            };

            var parameters = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "platformDetails",
                    platformDetails
                },
                {
                    "uploadDetails",
                    uploadDetails
                }
            };

            byte[] parametersBody = JsonSerializer.SerializeToUtf8Bytes(parameters);
            string encodedParameters = Convert.ToBase64String(parametersBody);

            var values = new Event {
                accountUuid = (Guid)accountUuid,
                url = url,
                eventType = "upload",
                parameters = encodedParameters
            };

            byte[] encodedValues = JsonSerializer.SerializeToUtf8Bytes(values);

            (int _statusCode, var response) = await _apiService.CreateEvent(encodedValues);
        }

        private async void OnSendDataClicked(object sender, EventArgs e)
        {
            await SendFiles();
        }

        public async void CreateUploadFile(string filePath, UploadFolder uploadFolder)
        {
            User currentUser = await _authenticate.getCurrentUser();
            if (currentUser != null)
            {
                DateTime createdAt = System.IO.File.GetCreationTime(filePath);
                DateTime updatedAt = System.IO.File.GetLastWriteTime(filePath);

                int userId = (int)currentUser.id;
                string _fileName = Path.GetFileName(filePath);
                string fileExt = Path.GetExtension(filePath);
                string mimeType = MimeTypeMapper.GetMimeType(fileExt);

                if (mimeType != "application/octet-stream")
                {
                    UploadFile uploadFile = new UploadFile
                    {
                        userId = userId,
                        uuid = Guid.NewGuid(),
                        createdAt = createdAt,
                        updatedAt = updatedAt,
                        filePath = filePath,
                        mimeType = mimeType,
                        source = uploadFolder.Type,
                    };

                    UploadFiles.Add(uploadFile);

                    UploadFilesCount++;

                    labelFilesCount.Text = AppResources.Count("FilesSelected", UploadFilesCount);
                    labelFilesCount.TextColor = Colors.White;

                    resetLink.TextColor = UploadFilesCount > 0 ? Colors.FloralWhite : Colors.Grey;
                    resetLinkImage.Color = UploadFilesCount > 0 ? Colors.FloralWhite : Colors.Grey;
                }
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
