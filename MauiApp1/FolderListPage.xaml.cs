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

public class PickerOption // Class to hold location data
{
    public string ID { get; set; }
    public string Name { get; set; }
}


namespace MauiApp1
{
    public partial class FolderListPage : ContentPage
    {
        public List<PickerOption> PickerOptions { get; set; }

        public ObservableCollection<Folder> Folders { get; set; }

        public ObservableCollection<Folder> SelectedFolders { get; set; }

        public int UploadFilesCount { get; set; }

        private readonly IFolderPicker _folderPicker;

        private readonly IApiService _apiService;

        private readonly IAlertService _alertService;

        private readonly AppShellViewModel _appShellViewModel;

        public FolderListPage(IFolderPicker folderPicker, IApiService apiService, IAlertService alertService, AppShellViewModel appShellViewModel)
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
            EventPicker.SelectedIndexChanged += new EventHandler(OnSelectedIndexChanged);

            PopulatePicker();

            getAllUploads();
        }

        private void RefreshFolders(IEnumerable<Folder> folders)
        {
            foldersCollectionView.ItemsSource = folders;

            int foldersCount = folders.Count();
            string folderLabel = foldersCount == 1 ? "Folder" : "Folders";
            foldersCountLabel.Text = Convert.ToString(folders.Count() + " " + folderLabel);
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            PickerOption selectedOption = null;
            foreach (var pickerOption in PickerOptions)
            {
                if (Convert.ToInt32(pickerOption.ID) == selectedIndex)
                {
                    selectedOption = pickerOption;
                    break;
                }
            }

            if (selectedOption != null)
            {
                if (selectedOption.Name == "All")
                {
                    var folders = Folders;

                    RefreshFolders(folders);

                    updatePublishButton();
                }
                else if (selectedOption.Name == "Published")
                {
                    var folders = Folders.Where(folder =>
                        folder.state == "published"
                    );

                    RefreshFolders(folders);

                    updatePublishButton();
                }
                else if (selectedOption.Name == "Archived")
                {
                    var folders = Folders.Where(folder =>
                        folder.state == "archived"
                    );

                    RefreshFolders(folders);

                    updatePublishButton();
                }
            }
        }

        private void PopulatePicker()
        {
            PickerOptions = new List<PickerOption>
            {
                new PickerOption { ID = "0", Name = "All" },
                new PickerOption { ID = "1", Name = "Published" },
                new PickerOption { ID = "2", Name = "Archived" }
            };

            foreach (var pickerOption in PickerOptions)
            {
                EventPicker.Items.Add(pickerOption.Name);
            }
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

            (int _statusCode, var response) = await _apiService.GetAllUploads("");
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

            foldersCollectionView.ItemsSource = Folders;

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

            updatePublishButton();
        }

        public void folderCheckedChanged(object sender, EventArgs e)
        {
            CheckBox selectFolder = (CheckBox)sender;

            updatePublishButton();
        }

        public void toggleCheckBox(object sender, EventArgs e)
        {
            Button nameButton = (Button)sender;

            var rootViewsAndTheirDescendants = foldersCollectionView.GetVisualTreeDescendants();

            foreach (VisualElement element in rootViewsAndTheirDescendants)
            {
                if (element is Microsoft.Maui.Controls.CheckBox)
                {
                    CheckBox checkbox = (CheckBox)element;
                    if (checkbox.ClassId == nameButton.ClassId)
                    {
                        checkbox.IsChecked = !checkbox.IsChecked;
                    }
                }
            }
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

        public async void updatePublishButton()
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

                publishFoldersButton.BackgroundColor = Colors.Orange;
                publishFoldersButton.TextColor = Colors.Black;
                publishFoldersButtonImage.Color = Colors.White;

                unpublishFoldersButton.BackgroundColor = Colors.Black;
                unpublishFoldersButton.TextColor = Colors.White;
                unpublishFoldersButtonImage.Color = Colors.White;

                deleteFoldersButton.BackgroundColor = Colors.Black;
                deleteFoldersButton.TextColor = Colors.White;
                deleteFoldersButtonImage.Color = Colors.White;
            }
            else
            {
                selectedFoldersCountLabel.Text = "No Folders selected";

                publishFoldersButton.BackgroundColor = Colors.Black;
                publishFoldersButton.TextColor = Colors.Gray;
                publishFoldersButtonImage.Color = Colors.Gray;

                unpublishFoldersButton.BackgroundColor = Colors.Black;
                unpublishFoldersButton.TextColor = Colors.Gray;
                unpublishFoldersButtonImage.Color = Colors.Gray;

                deleteFoldersButton.BackgroundColor = Colors.Black;
                deleteFoldersButton.TextColor = Colors.Gray;
                deleteFoldersButtonImage.Color = Colors.Gray;
            }
        }

        public void accountLinkClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("accountpage");
        }

        public void refreshButtonClicked(object sender, EventArgs e)
        {
            getAllUploads();

            updatePublishButton();
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

        public async void publishFoldersButtonClicked(object sender, EventArgs e)
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

            bool confirm = await _alertService.DisplayAlertAsync(
               title: "Publish Folders",
               message: String.Join("\n", FolderNames),
               accept: "OK",
               cancel: "Cancel");

            if (confirm)
            {
                var folderIds = new CollectionIds
                {
                    id = ids.ToArray()
                };

                byte[] body = JsonSerializer.SerializeToUtf8Bytes(folderIds);
                (int _statusCode, var response) = await _apiService.PublishFolders(body);

                folderSearchBar.Text = "";

                EventPicker.SelectedIndex = 0;

                getAllUploads();

                uncheckCheckBoxes();

                updatePublishButton();
            }
        }

        public async void unpublishFoldersButtonClicked(object sender, EventArgs e)
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

            bool confirm = await _alertService.DisplayAlertAsync(
               title: "Unpublish Folders",
               message: String.Join("\n", FolderNames),
               accept: "OK",
               cancel: "Cancel");

            if (confirm)
            {
                var folderIds = new CollectionIds
                {
                    id = ids.ToArray()
                };

                byte[] body = JsonSerializer.SerializeToUtf8Bytes(folderIds);
                (int _statusCode, var response) = await _apiService.UnpublishFolders(body);

                folderSearchBar.Text = "";

                EventPicker.SelectedIndex = 0;

                getAllUploads();

                uncheckCheckBoxes();

                updatePublishButton();
            }
        }

        public async void deleteFoldersButtonClicked(object sender, EventArgs e)
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

            bool confirm = await _alertService.DisplayAlertAsync(
               title: "Delete Folders",
               message: String.Join("\n", FolderNames),
               accept: "OK",
               cancel: "Cancel");

            if (confirm)
            {
                var folderIds = new CollectionIds
                {
                    id = ids.ToArray()
                };

                byte[] body = JsonSerializer.SerializeToUtf8Bytes(folderIds);
                (int _statusCode, var response) = await _apiService.DeleteFolders(body);

                folderSearchBar.Text = "";

                EventPicker.SelectedIndex = 0;

                getAllUploads();

                uncheckCheckBoxes();

                updatePublishButton();
            }
        }

        public async void foldersSearchInputTextChanged(object sender, EventArgs e)
        {
            SearchBar searchBar = (SearchBar)sender;

            var folders = Folders.Where(folder =>
                folder.name.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)
            );

            EventPicker.SelectedIndex = 0;

            RefreshFolders(folders);

            uncheckCheckBoxes();

            updatePublishButton();
        }
    }
}
