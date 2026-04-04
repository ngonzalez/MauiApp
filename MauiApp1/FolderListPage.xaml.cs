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

public class PickerOption
{
    public string ID { get; set; }
    public string Name { get; set; }
}

public class CollectionIds
{
    public string[] id { get; set; }
}

namespace MauiApp1
{
    public partial class FolderListPage : ContentPage
    {
        public List<PickerOption> FolderStatePickerOptions { get; set; }

        public List<PickerOption> FolderActionPickerOptions { get; set; }

        public ObservableCollection<Folder> Folders { get; set; }

        public ObservableCollection<Folder> SelectedFolders { get; set; }

        public int UploadFilesCount { get; set; }

        private readonly IFolderPicker _folderPicker;

        private readonly IAlertService _alertService;

        private readonly IApiService _apiService;

        private readonly AppShellViewModel _appShellViewModel;

        public FolderListPage(IFolderPicker folderPicker, IAlertService alertService, IApiService apiService, AppShellViewModel appShellViewModel)
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

            Folders = new ObservableCollection<Folder> { };
            SelectedFolders = new ObservableCollection<Folder> { };

            InitializeComponent();

            BindingContext = this;

            myAccountLink.Clicked += new EventHandler(accountLinkClicked);
            refreshFilesButton.Clicked += new EventHandler(refreshButtonClicked);
            selectAllFolders.Clicked += new EventHandler(selectAllFoldersButtonClicked);
            FolderStatePicker.SelectedIndexChanged += new EventHandler(FolderStatePickerOnSelectedIndexChanged);
            FolderActionPicker.SelectedIndexChanged += new EventHandler(FolderActionPickerOnSelectedIndexChanged);

            selectedFoldersCountLabel.Text = "No Folders selected";

            PopulateFolderStatePicker();

            PopulateFolderActionPicker();

            getUploads();
        }

        private void RefreshFolders(IEnumerable<Folder> folders)
        {
            foldersCollectionView.ItemsSource = folders;
            int foldersCount = folders.Count();
            string folderLabel = foldersCount == 1 ? "Folder" : "Folders";
            foldersCountLabel.Text = Convert.ToString(folders.Count() + " " + folderLabel);
        }

        private void FolderStatePickerOnSelectedIndexChanged(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            PickerOption selectedOption = null;
            foreach (var pickerOption in FolderStatePickerOptions)
            {
                if (Convert.ToInt32(pickerOption.ID) == selectedIndex)
                {
                    selectedOption = pickerOption;
                    break;
                }
            }

            if (selectedOption != null)
            {
                if (selectedOption.Name == "")
                {
                    var folders = Folders.Where(folder =>
                        folder.state == "created" || folder.state == "published"
                    );

                    RefreshFolders(folders);

                    updateFoldersActionButton();

                    FolderStatePicker.TextColor = Colors.Gray;
                }
                else if (selectedOption.Name == "Published")
                {
                    var folders = Folders.Where(folder =>
                        folder.state == "published"
                    );

                    RefreshFolders(folders);

                    updateFoldersActionButton();

                    FolderStatePicker.TextColor = Colors.FloralWhite;
                }
                else if (selectedOption.Name == "Archived")
                {
                    var folders = Folders.Where(folder =>
                        folder.state == "archived"
                    );

                    RefreshFolders(folders);

                    updateFoldersActionButton();

                    FolderStatePicker.TextColor = Colors.FloralWhite;
                }
            }
        }

        private void FolderActionPickerOnSelectedIndexChanged(object sender, EventArgs e)
        {
            updateFoldersActionButton();
        }

        private void PopulateFolderStatePicker()
        {
            FolderStatePickerOptions = new List<PickerOption>
            {
                new PickerOption { ID = "0", Name = "" },
                new PickerOption { ID = "1", Name = "Published" },
                new PickerOption { ID = "2", Name = "Archived" }
            };

            foreach (var pickerOption in FolderStatePickerOptions)
            {
                FolderStatePicker.Items.Add(pickerOption.Name);
            }
        }

        private void PopulateFolderActionPicker()
        {
            FolderActionPickerOptions = new List<PickerOption>
            {
                new PickerOption { ID = "0", Name = "" },
                new PickerOption { ID = "1", Name = "Publish Folders" },
                new PickerOption { ID = "2", Name = "Unpublish Folders" },
                new PickerOption { ID = "3", Name = "Archive Folders" },
                new PickerOption { ID = "4", Name = "Unarchive Folders" },
                new PickerOption { ID = "5", Name = "Delete Folders" },
            };

            foreach (var pickerOption in FolderActionPickerOptions)
            {
                FolderActionPicker.Items.Add(pickerOption.Name);
            }
        }

        public async void displayFiles(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Folder folder = (Folder)button.BindingContext;
            Window secondWindow = new Window(new DisplayPage(_alertService, _apiService, _appShellViewModel, folder));
            App.Current.OpenWindow(secondWindow);
        }

        public async void getUploads()
        {
            ActivityIndicator.IsRunning = true;

            (int _statusCode, var response) = await _apiService.getUploads("");
            var uploadsResponse = JsonSerializer.Deserialize<Upload[]>(response);

            while (Folders.Count() > 0)
            {
                Folders.RemoveAt(0);
            }

            Folders = new ObservableCollection<Folder> { };

            foreach (var item in uploadsResponse)
            {
                if (item.imageFiles.Length > 0)
                {
                    foreach (ImageFile imageFile in item.imageFiles)
                    {
                        if (imageFile.folder != null)
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.id == imageFile.folder.id)
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
                        if (textFile.folder != null)
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.id == textFile.folder.id)
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
                        if (pdfFile.folder != null)
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.id == pdfFile.folder.id)
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
                        if (audioFile.folder != null)
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.id == audioFile.folder.id)
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
                        if (videoFile.folder != null)
                        {
                            bool found = false;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.id == videoFile.folder.id)
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

            foldersCollectionView.ItemsSource = Folders.Where(folder =>
                folder.state == "created" || folder.state == "published"
            );

            foldersCountLabel.Text = Convert.ToString(Folders.Count() + " Folders");
        }

        public async void FolderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Folder selectedFolder = e.CurrentSelection.FirstOrDefault() as Folder;

            SelectedFolders.Add(selectedFolder);
        }

        public void selectAllFoldersButtonClicked(object sender, EventArgs e)
        {
            Button selectAllFolders = (Button)sender;

            toggleCheckBoxes();

            updateFoldersActionButton();
        }

        public void folderCheckedChanged(object sender, EventArgs e)
        {
            CheckBox selectFolder = (CheckBox)sender;

            updateFoldersActionButton();
        }

        public void uncheckCheckBoxes()
        {
            var rootViewsAndTheirDescendants = foldersCollectionView.GetVisualTreeDescendants();

            foreach (VisualElement element in rootViewsAndTheirDescendants)
            {
                if (element is Microsoft.Maui.Controls.CheckBox)
                {
                    CheckBox checkbox = (CheckBox)element;
                    checkbox.IsChecked = false;
                }
            }
        }

        public void toggleCheckBoxes()
        {
            var rootViewsAndTheirDescendants = foldersCollectionView.GetVisualTreeDescendants();

            bool allChecked = true;
            foreach (VisualElement element in rootViewsAndTheirDescendants)
            {
                if (element is Microsoft.Maui.Controls.CheckBox)
                {
                    CheckBox checkbox = (CheckBox)element;
                    allChecked = checkbox.IsChecked;
                }
            }
            foreach (VisualElement element in rootViewsAndTheirDescendants)
            {
                if (element is Microsoft.Maui.Controls.CheckBox)
                {
                    CheckBox checkbox = (CheckBox)element;
                    checkbox.IsChecked = !allChecked;
                }
            }
        }

        public async void updateFoldersActionButton()
        {
            var rootViewsAndTheirDescendants = foldersCollectionView.GetVisualTreeDescendants();

            int foldersCount = 0;
            foreach (VisualElement element in rootViewsAndTheirDescendants)
            {
                if (element is Microsoft.Maui.Controls.CheckBox)
                {
                    CheckBox checkbox = (CheckBox)element;
                    if (checkbox.IsChecked)
                    {
                        foldersCount++;
                    }
                }
            }

            if (foldersCount > 0)
            {
                string folderLabel = foldersCount == 1 ? "Folder" : "Folders";
                selectedFoldersCountLabel.Text = Convert.ToString(foldersCount) + " " + folderLabel + " selected";
            }
            else
            {
                selectedFoldersCountLabel.Text = "No Folders selected";
            }

            PickerOption selectedOption = null;
            foreach (var pickerOption in FolderActionPickerOptions)
            {
                if (Convert.ToInt32(pickerOption.ID) == FolderActionPicker.SelectedIndex)
                {
                    selectedOption = pickerOption;
                    break;
                }
            }

            if (foldersCount > 0 && (selectedOption != null && selectedOption.Name != ""))
            {
                updateFoldersButton.BackgroundColor = Colors.Orange;
                updateFoldersButton.TextColor = Colors.Black;
                updateFoldersButtonImage.Color = Colors.White;
                FolderActionPicker.TextColor = Colors.FloralWhite;
            }
            else
            {
                updateFoldersButton.BackgroundColor = Colors.Black;
                updateFoldersButton.TextColor = Colors.Gray;
                updateFoldersButtonImage.Color = Colors.Gray;
                FolderActionPicker.TextColor = Colors.Gray;
            }
        }

        public void accountLinkClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("accountpage");
        }

        public void refreshButtonClicked(object sender, EventArgs e)
        {
            folderSearchBar.Text = "";

            FolderStatePicker.SelectedIndex = 0;

            getUploads();

            updateFoldersActionButton();
        }

        public List<string> getSelectedFolderIds()
        {
            List<string> ids = new List<string>();
            var rootViewsAndTheirDescendants = foldersCollectionView.GetVisualTreeDescendants();
            foreach (VisualElement element in rootViewsAndTheirDescendants)
            {
                if (element is Microsoft.Maui.Controls.CheckBox)
                {
                    CheckBox checkbox = (CheckBox)element;
                    if (checkbox.IsChecked)
                    {
                        ids.Add(checkbox.ClassId);
                    }
                }
            }
            return ids;
        }

        public async void updateFoldersButtonClicked(object sender, EventArgs e)
        {
            List<string> ids = getSelectedFolderIds();
            List<string> FolderNames = new List<string>();
            foreach (Folder folder in Folders)
            {
                if (ids.Contains(Convert.ToString(folder.id)))
                {
                    FolderNames.Add(folder.name);
                }
            }

            PickerOption selectedOption = null;
            foreach (var pickerOption in FolderActionPickerOptions)
            {
                if (Convert.ToInt32(pickerOption.ID) == FolderActionPicker.SelectedIndex)
                {
                    selectedOption = pickerOption;
                    break;
                }
            }

            if (selectedOption != null)
            {
                string action = await DisplayActionSheet(selectedOption.Name, "Cancel", selectedOption.Name, String.Join("\n", FolderNames));

                if (action == selectedOption.Name)
                {
                    var folderIds = new CollectionIds
                    {
                        id = ids.ToArray()
                    };

                    byte[] body = JsonSerializer.SerializeToUtf8Bytes(folderIds);

                    if (selectedOption.Name == "Publish Folders")
                    {
                        (int _statusCode, var response) = await _apiService.PublishFolders(body);
                    }
                    else if (selectedOption.Name == "Unpublish Folders")
                    {
                        (int _statusCode, var response) = await _apiService.UnpublishFolders(body);
                    }
                    else if (selectedOption.Name == "Archive Folders")
                    {
                        (int _statusCode, var response) = await _apiService.ArchiveFolders(body);
                    }
                    else if (selectedOption.Name == "Unarchive Folders")
                    {
                        (int _statusCode, var response) = await _apiService.UnarchiveFolders(body);
                    }
                    else if (selectedOption.Name == "Delete Folders")
                    {
                        (int _statusCode, var response) = await _apiService.DeleteFolders(body);
                    }

                    folderSearchBar.Text = "";

                    FolderStatePicker.SelectedIndex = 0;

                    FolderActionPicker.SelectedIndex = 0;

                    getUploads();

                    uncheckCheckBoxes();

                    updateFoldersActionButton();
                }
            }
        }

        public async void foldersSearchInputTextChanged(object sender, EventArgs e)
        {
            SearchBar searchBar = (SearchBar)sender;

            var folders = Folders.Where(folder =>
                folder.name.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase) &&
                (folder.state == "created" || folder.state == "published")
            );

            FolderStatePicker.SelectedIndex = 0;

            FolderActionPicker.SelectedIndex = 0;

            RefreshFolders(folders);

            uncheckCheckBoxes();

            updateFoldersActionButton();
        }

        private string getFolderURL(Folder folder)
        {
            string url = "https://link12.ddns.net/" + folder.dataUrl;
            return url;
        }

        public void SetTimeout(Action action, int ms)
        {
            Task.Delay(ms).ContinueWith((task) =>
            {
                action();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void OpenWebURL_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button button = (Button)sender;
                Folder folder = (Folder)button.BindingContext;
                string url = getFolderURL(folder);
                await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(url);
            }
            catch (Exception _ex)
            {
                //
            }
        }

        private async void SetClipboardButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button button = (Button)sender;
                Folder folder = (Folder)button.BindingContext;
                string url = getFolderURL(folder);
                await Clipboard.Default.SetTextAsync(url);
            }
            catch (Exception _ex)
            {
                //
            }

            actionLabel.Text = "Folder URL copied to clipboard";
            SetTimeout(() =>
                {
                    actionLabel.Text = "";
                }, 2000);
            }
        }
    }
