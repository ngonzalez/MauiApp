using CommunityToolkit.Maui.Core.Primitives;
using MauiApp1.Platforms.Windows;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using Windows.System;

namespace MauiApp1;

public class PickerOption
{
    public string ID { get; set; }
    public string Name { get; set; }
}

public class CollectionIds
{
    public string[] id { get; set; }

    public string[] type { get; set; }
}

public class VideoStreamResponse
{
    public int id { get; set; }
    public bool m3u8Exists { get; set; }

}
public class AudioStreamResponse
{
    public int id { get; set; }
    public bool m3u8Exists { get; set; }

}
public partial class DisplayPage : ContentPage
{
    private readonly IAlertService _alertService;

    private readonly IApiService _apiService;

    private readonly AppShellViewModel _appShellViewModel;

    private Folder _folder;

    public List<PickerOption> ImageFileActionPickerOptions { get; set; }
    public List<PickerOption> VideoFileActionPickerOptions { get; set; }
    public List<PickerOption> AudioFileActionPickerOptions { get; set; }

    public ObservableCollection<ImageFile> ImageFiles { get; set; }
    public ObservableCollection<ImageFile> SelectedImageFiles { get; set; }

    public ObservableCollection<VideoFile> VideoFiles { get; set; }
    public ObservableCollection<VideoFile> SelectedVideoFiles { get; set; }
    public ObservableCollection<VideoStreamResponse> VideoStreams { get; set; }

    public ObservableCollection<AudioFile> AudioFiles { get; set; }
    public ObservableCollection<AudioFile> SelectedAudioFiles { get; set; }
    public ObservableCollection<AudioStreamResponse> AudioStreams { get; set; }

    public ObservableCollection<PdfFile> PdfFiles { get; set; }
    public ObservableCollection<PdfFile> SelectedPdfFiles { get; set; }

    public ObservableCollection<TextFile> TextFiles { get; set; }
    public ObservableCollection<TextFile> SelectedTextFiles { get; set; }

    public DisplayPage(IAlertService alertService, IApiService apiService, AppShellViewModel appShellViewModel, Folder folder)
    {
        _alertService = alertService;
        _apiService = apiService;
        _appShellViewModel = appShellViewModel;
        _folder = folder;

        // ImageFile collection
        ImageFiles = new ObservableCollection<ImageFile> { };
        SelectedImageFiles = new ObservableCollection<ImageFile> { };

        // VideoFile collection
        VideoFiles = new ObservableCollection<VideoFile> { };
        SelectedVideoFiles = new ObservableCollection<VideoFile> { };

        // VideoStreams collection
        VideoStreams = new ObservableCollection<VideoStreamResponse> { };

        // AudioFile collection
        AudioFiles = new ObservableCollection<AudioFile> { };
        SelectedAudioFiles = new ObservableCollection<AudioFile> { };

        // AudioStreams collection
        AudioStreams = new ObservableCollection<AudioStreamResponse> { };

        // PdfFile collection
        PdfFiles = new ObservableCollection<PdfFile> { };
        SelectedPdfFiles = new ObservableCollection<PdfFile> { };

        // TextFile collection
        TextFiles = new ObservableCollection<TextFile> { };
        SelectedTextFiles = new ObservableCollection<TextFile> { };

        InitializeComponent();
        BindingContext = this;

        // Set folder name
        folderName1.Text = folder.name;
        folderName2.Text = folder.name;
        folderName3.Text = folder.name;
        folderName4.Text = folder.name;
        folderName5.Text = folder.name;

        // Set default visibility for grids
        GridImageFiles.IsVisible = false;
        GridImageFilesDetails.IsVisible = false;
        GridVideoFiles.IsVisible = false;
        GridVideoFilesDetails.IsVisible = false;
        GridAudioFiles.IsVisible = false;
        GridAudioFilesDetails.IsVisible = false;
        GridMediaPlayer.IsVisible = false;
        GridMediaProcessing.IsVisible = false;
        GridPdfFiles.IsVisible = false;
        GridPdfFilesDetails.IsVisible = false;
        GridTextFiles.IsVisible = false;
        GridTextFilesDetails.IsVisible = false;

        // Image File Actions
        selectAllImageFiles.Clicked += new EventHandler(selectAllImageFilesButtonClicked);
        ImageFileActionPicker.SelectedIndexChanged += new EventHandler(ImageFileActionPickerOnSelectedIndexChanged);
        selectedImageFilesCountLabel.Text = "No Image Files selected";
        refreshImageFilesButton.Clicked += new EventHandler(refreshImageFilesButtonClicked);
        PopulateImageFilesActionPicker();

        // Video File Actions
        selectAllVideoFiles.Clicked += new EventHandler(selectAllVideoFilesButtonClicked);
        VideoFileActionPicker.SelectedIndexChanged += new EventHandler(VideoFileActionPickerOnSelectedIndexChanged);
        selectedVideoFilesCountLabel.Text = "No Video Files selected";
        refreshVideoFilesButton.Clicked += new EventHandler(refreshVideoFilesButtonClicked);
        PopulateVideoFilesActionPicker();

        // Audio File Actions
        selectAllAudioFiles.Clicked += new EventHandler(selectAllAudioFilesButtonClicked);
        AudioFileActionPicker.SelectedIndexChanged += new EventHandler(AudioFileActionPickerOnSelectedIndexChanged);
        selectedAudioFilesCountLabel.Text = "No Audio Files selected";
        refreshAudioFilesButton.Clicked += new EventHandler(refreshAudioFilesButtonClicked);
        PopulateAudioFilesActionPicker();

        // Get media files from backend
        getUploads();
    }

    public async void getUploads()
    {
        string folderId = Convert.ToString(_folder.id);
        var folderIdsUtf8 = JsonSerializer.SerializeToUtf8Bytes("," + folderId);
        string encodedFolderId = Convert.ToBase64String(folderIdsUtf8);
        (int _statusCode, var response) = await _apiService.GetAllUploads("?folderIds=" + encodedFolderId);
        var uploadsResponse = JsonSerializer.Deserialize<Upload[]>(response);

        // ImageFile
        ImageFiles = new ObservableCollection<ImageFile> { };

        while (ImageFiles.Count() > 0)
        {
            ImageFiles.RemoveAt(0);
        }

        foreach (Upload upload in uploadsResponse)
        {
            if (upload.imageFiles.Length > 0)
            {
                foreach (ImageFile imageFile in upload.imageFiles)
                {
                    ImageFiles.Add(imageFile);
                }
            }
        }

        imageFilesCollectionView.ItemsSource = ImageFiles;
        string imageFileLabel = ImageFiles.Count() > 1 ? "Image Files" : "Image File";
        imageFilesCount.Text = Convert.ToString(ImageFiles.Count()) + " " + imageFileLabel;
        GridImageFiles.IsVisible = ImageFiles.Count() > 0;
        GridImageFilesDetails.IsVisible = ImageFiles.Count() > 0;
        imageFilesCountLabel.Text = Convert.ToString(ImageFiles.Count() + " " + imageFileLabel);

        // VideoFile
        VideoFiles = new ObservableCollection<VideoFile> { };

        while (VideoFiles.Count() > 0)
        {
            VideoFiles.RemoveAt(0);
        }

        foreach (Upload upload in uploadsResponse)
        {
            if (upload.videoFiles.Length > 0)
            {
                foreach (VideoFile videoFile in upload.videoFiles)
                {
                    VideoFiles.Add(videoFile);
                }
            }
        }

        videoFilesCollectionView.ItemsSource = VideoFiles;
        string videoFileLabel = VideoFiles.Count() > 1 ? "Video Files" : "Video File";
        videoFilesCount.Text = Convert.ToString(VideoFiles.Count()) + " " + videoFileLabel;
        GridVideoFiles.IsVisible = VideoFiles.Count() > 0;
        GridVideoFilesDetails.IsVisible = VideoFiles.Count() > 0;
        videoFilesCountLabel.Text = Convert.ToString(VideoFiles.Count() + " " + videoFileLabel);

        // AudioFile
        AudioFiles = new ObservableCollection<AudioFile> { };

        while (AudioFiles.Count() > 0)
        {
            AudioFiles.RemoveAt(0);
        }

        foreach (Upload upload in uploadsResponse)
        {
            if (upload.audioFiles.Length > 0)
            {
                foreach (AudioFile audioFile in upload.audioFiles)
                {
                    AudioFiles.Add(audioFile);
                }
            }
        }

        audioFilesCollectionView.ItemsSource = AudioFiles;
        string audioFileLabel = AudioFiles.Count() > 1 ? "Audio Files" : "Audio File";
        audioFilesCount.Text = Convert.ToString(AudioFiles.Count()) + " " + audioFileLabel;
        GridAudioFiles.IsVisible = AudioFiles.Count() > 0;
        GridAudioFilesDetails.IsVisible = AudioFiles.Count() > 0;
        audioFilesCountLabel.Text = Convert.ToString(AudioFiles.Count() + " " + audioFileLabel);

        // PdfFile
        PdfFiles = new ObservableCollection<PdfFile> { };

        while (PdfFiles.Count() > 0)
        {
            PdfFiles.RemoveAt(0);
        }

        foreach (Upload upload in uploadsResponse)
        {
            if (upload.pdfFiles.Length > 0)
            {
                foreach (PdfFile pdfFile in upload.pdfFiles)
                {
                    PdfFiles.Add(pdfFile);
                }
            }
        }

        pdfFilesCollectionView.ItemsSource = PdfFiles;
        string pdfFileLabel = PdfFiles.Count() > 1 ? "Pdf Files" : "Pdf File";
        pdfFilesCount.Text = Convert.ToString(PdfFiles.Count()) + " " + pdfFileLabel;
        GridPdfFiles.IsVisible = PdfFiles.Count() > 0;
        GridPdfFilesDetails.IsVisible = PdfFiles.Count() > 0;

        // TextFile
        TextFiles = new ObservableCollection<TextFile> { };

        while (TextFiles.Count() > 0)
        {
            TextFiles.RemoveAt(0);
        }

        foreach (Upload upload in uploadsResponse)
        {
            if (upload.textFiles.Length > 0)
            {
                foreach (TextFile textFile in upload.textFiles)
                {
                    TextFiles.Add(textFile);
                }
            }
        }

        textFilesCollectionView.ItemsSource = TextFiles;
        string textFileLabel = TextFiles.Count() > 1 ? "Text Files" : "Text File";
        textFilesCount.Text = Convert.ToString(TextFiles.Count()) + " " + textFileLabel;
        GridTextFiles.IsVisible = TextFiles.Count() > 0;
        GridTextFilesDetails.IsVisible = TextFiles.Count() > 0;
    }

    public async void openNewWindowImageFile(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        ImageFile imageFile = (ImageFile)button.BindingContext;
        Window secondWindow = new Window(new ShowImageFilePage(_apiService, _appShellViewModel, imageFile));
        App.Current.OpenWindow(secondWindow);
    }

    public async void openNewWindowPdfFile(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        PdfFile pdfFile = (PdfFile)button.BindingContext;
        string url = "https://link12.ddns.net/";
        url += pdfFile.folder.dataUrl;
        url += "/pdfWebView/";
        url += pdfFile.dataUrl;
        try
        {
            Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(url);
        }
        catch (Exception _ex)
        {
            //
        }
    }

    public async void openNewWindowTextFile(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        TextFile textFile = (TextFile)button.BindingContext;
        string url = "https://link12.ddns.net/";
        url += textFile.folder.dataUrl;
        url += "/textWebView/";
        url += textFile.dataUrl;
        try
        {
            Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(url);
        }
        catch (Exception _ex)
        {
            //
        }
    }

    public async void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        Console.WriteLine($"ScrollX: {e.ScrollX}, ScrollY: {e.ScrollY}");
    }

    public List<string> getSelectedImageFileIds()
    {
        List<string> ids = new List<string>();
        var rootViewsAndTheirDescendants = imageFilesCollectionView.GetVisualTreeDescendants();
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

    public async void updateImageFilesButtonClicked(object sender, EventArgs e)
    {
        List<string> ids = getSelectedImageFileIds();
        List<string> ImageFileNames = new List<string>();
        foreach (ImageFile imageFile in ImageFiles)
        {
            if (ids.Contains(Convert.ToString(imageFile.id)))
            {
                ImageFileNames.Add(imageFile.fileName);
            }
        }

        PickerOption selectedOption = null;
        foreach (var pickerOption in ImageFileActionPickerOptions)
        {
            if (Convert.ToInt32(pickerOption.ID) == ImageFileActionPicker.SelectedIndex)
            {
                selectedOption = pickerOption;
                break;
            }
        }

        if (selectedOption != null)
        {
            string action = await DisplayActionSheet(selectedOption.Name, "Cancel", "Delete", String.Join("\n", ImageFileNames));

            if (action == "Delete")
            {
                var imageFileIds = new CollectionIds
                {
                    id = ids.ToArray(),
                    type = ["ImageFile"]
                };

                byte[] body = JsonSerializer.SerializeToUtf8Bytes(imageFileIds);

                if (selectedOption.Name == "Delete Image Files")

                {
                    (int _statusCode, var response) = await _apiService.DeleteAttachments(body);
                }

                imageFilesSearchBar.Text = "";

                ImageFileActionPicker.SelectedIndex = 0;

                getUploads();

                uncheckImageFileCheckBoxes();

                updateImageFilesActionButton();

                while (SelectedImageFiles.Count() > 0)
                {
                    SelectedImageFiles.RemoveAt(0);
                }
            }
        }
    }

    public void refreshImageFilesButtonClicked(object sender, EventArgs e)
    {
        imageFilesSearchBar.Text = "";

        getUploads();

        updateImageFilesActionButton();
    }


    private void ImageFileActionPickerOnSelectedIndexChanged(object sender, EventArgs e)
    {
        updateImageFilesActionButton();
    }

    public void selectAllImageFilesButtonClicked(object sender, EventArgs e)
    {
        Button selectAllFolders = (Button)sender;

        toggleImageFilesCheckBoxes();

        updateImageFilesActionButton();
    }
    private void PopulateImageFilesActionPicker()
    {
        ImageFileActionPickerOptions = new List<PickerOption>
            {
                new PickerOption { ID = "0", Name = "" },
                new PickerOption { ID = "1", Name = "Delete Image Files" },
            };

        foreach (var pickerOption in ImageFileActionPickerOptions)
        {
            ImageFileActionPicker.Items.Add(pickerOption.Name);
        }
    }

    public void uncheckImageFileCheckBoxes()
    {
        var rootViewsAndTheirDescendants = imageFilesCollectionView.GetVisualTreeDescendants();

        foreach (VisualElement element in rootViewsAndTheirDescendants)
        {
            if (element is Microsoft.Maui.Controls.CheckBox)
            {
                CheckBox checkbox = (CheckBox)element;
                checkbox.IsChecked = false;
            }
        }
    }

    public void toggleImageFilesCheckBoxes()
    {
        var rootViewsAndTheirDescendants = imageFilesCollectionView.GetVisualTreeDescendants();

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

    public void imageFileCheckedChanged(object sender, EventArgs e)
    {
        CheckBox selectFolder = (CheckBox)sender;

        updateImageFilesActionButton();
    }

    public async void updateImageFilesActionButton()
    {
        var rootViewsAndTheirDescendants = imageFilesCollectionView.GetVisualTreeDescendants();

        int imageFilesCount = 0;
        foreach (VisualElement element in rootViewsAndTheirDescendants)
        {
            if (element is Microsoft.Maui.Controls.CheckBox)
            {
                CheckBox checkbox = (CheckBox)element;
                if (checkbox.IsChecked)
                {
                    imageFilesCount++;
                }
            }
        }

        if (imageFilesCount > 0)
        {
            string imageFilesLabel = imageFilesCount == 1 ? "Image File" : "Image Files";
            selectedImageFilesCountLabel.Text = Convert.ToString(imageFilesCount) + " " + imageFilesLabel + " selected";
        }
        else
        {
            selectedImageFilesCountLabel.Text = "No Image Files selected";
        }

        PickerOption selectedOption = null;
        foreach (var pickerOption in ImageFileActionPickerOptions)
        {
            if (Convert.ToInt32(pickerOption.ID) == ImageFileActionPicker.SelectedIndex)
            {
                selectedOption = pickerOption;
                break;
            }
        }

        if (imageFilesCount > 0 && (selectedOption != null && selectedOption.Name != ""))
        {
            updateImageFilesButton.BackgroundColor = Colors.Orange;
            updateImageFilesButton.TextColor = Colors.Black;
            updateImageFilesButtonImage.Color = Colors.White;
            ImageFileActionPicker.TextColor = Colors.FloralWhite;
        }
        else
        {
            updateImageFilesButton.BackgroundColor = Colors.Black;
            updateImageFilesButton.TextColor = Colors.Gray;
            updateImageFilesButtonImage.Color = Colors.Gray;
            ImageFileActionPicker.TextColor = Colors.Gray;
        }
    }

    public async void imageFilesSearchInputTextChanged(object sender, EventArgs e)
    {
        SearchBar searchBar = (SearchBar)sender;
        imageFilesCollectionView.ItemsSource = ImageFiles.Where(imageFile =>
            imageFile.fileName.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)
        );

        ImageFileActionPicker.SelectedIndex = 0;

        uncheckImageFileCheckBoxes();

        updateImageFilesActionButton();

    }

    public async void addImageFileToSelectedItems(ImageFile imageFile)
    {
        SelectedImageFiles.Add(imageFile);
    }

    public async void previousLinkImageFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        ImageFile imageFile = (ImageFile)button.BindingContext;

        while (SelectedImageFiles.Count() > 0)
        {
            SelectedImageFiles.RemoveAt(0);
        }

        try
        {
            ImageFile nextImageFile = ImageFiles[ImageFiles.IndexOf(imageFile) - 1];

            foreach (ImageFile _imageFile in ImageFiles)
            {
                if (_imageFile.id == nextImageFile.id)
                {
                    addImageFileToSelectedItems(_imageFile);
                    break;
                }
            }
        }
        catch
        {

        }
    }

    public async void nextLinkImageFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        ImageFile imageFile = (ImageFile)button.BindingContext;

        while (SelectedImageFiles.Count() > 0)
        {
            SelectedImageFiles.RemoveAt(0);
        }

        try
        {
            ImageFile nextImageFile = ImageFiles[ImageFiles.IndexOf(imageFile) + 1];

            foreach (ImageFile _imageFile in ImageFiles)
            {
                if (_imageFile.id == nextImageFile.id)
                {
                    addImageFileToSelectedItems(_imageFile);
                    break;
                }
            }
        }
        catch
        {

        }
    }

    public async void removeImageFileFromSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        ImageFile imageFile = (ImageFile)button.BindingContext;
        int i = 0;
        foreach (ImageFile _imageFile in SelectedImageFiles)
        {
            if (_imageFile.id == imageFile.id)
            {
                SelectedImageFiles.RemoveAt(i);
                break;
            }
            i += 1;
        }
    }

    public async void addImageFileToSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        ImageFile imageFile = (ImageFile)button.BindingContext;

        while (SelectedImageFiles.Count() > 0)
        {
            SelectedImageFiles.RemoveAt(0);
        }

        addImageFileToSelectedItems(imageFile);
    }

    public async void SelectedImageFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ImageFile _selectedImageFile = e.CurrentSelection.FirstOrDefault() as ImageFile;
    }

    public async void ImageFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ImageFile _selectedImageFile = e.CurrentSelection.FirstOrDefault() as ImageFile;
    }

    public string getImageFileURL(ImageFile imageFile)
    {
        string url = "https://link12.ddns.net/" +
            imageFile.folder.dataUrl +
            "/image/" + imageFile.dataUrl;
        return url;
    }

    private async void ImageFileOpenWebURL_Clicked(object sender, EventArgs e)
    {
        try
        {
            Button button = (Button)sender;
            ImageFile imageFile = (ImageFile)button.BindingContext;
            string url = getImageFileURL(imageFile);
            await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(url);
        }
        catch (Exception _ex)
        {
            //
        }
    }

    private async void ImageFileSetClipboardButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            Button button = (Button)sender;
            ImageFile imageFile = (ImageFile)button.BindingContext;
            string url = getImageFileURL(imageFile);
            await Clipboard.Default.SetTextAsync(url);
        }
        catch (Exception _ex)
        {
            //
        }

        ImageFileActionLabel.Text = "Image File URL copied to clipboard";

        SetTimeout(() =>
        {
            ImageFileActionLabel.Text = "";
        }, 2000);
    }

    public List<string> getSelectedVideoFileIds()
    {
        List<string> ids = new List<string>();
        var rootViewsAndTheirDescendants = videoFilesCollectionView.GetVisualTreeDescendants();
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

    public async void updateVideoFilesButtonClicked(object sender, EventArgs e)
    {
        List<string> ids = getSelectedVideoFileIds();
        List<string> VideoFileNames = new List<string>();
        foreach (VideoFile videoFile in VideoFiles)
        {
            if (ids.Contains(Convert.ToString(videoFile.id)))
            {
                VideoFileNames.Add(videoFile.fileName);
            }
        }

        PickerOption selectedOption = null;
        foreach (var pickerOption in VideoFileActionPickerOptions)
        {
            if (Convert.ToInt32(pickerOption.ID) == VideoFileActionPicker.SelectedIndex)
            {
                selectedOption = pickerOption;
                break;
            }
        }

        if (selectedOption != null)
        {
            string action = await DisplayActionSheet(selectedOption.Name, "Cancel", "Delete", String.Join("\n", VideoFileNames));

            if (action == "Delete")
            {
                var videoFileIds = new CollectionIds
                {
                    id = ids.ToArray(),
                    type = ["VideoFile"]
                };

                byte[] body = JsonSerializer.SerializeToUtf8Bytes(videoFileIds);

                if (selectedOption.Name == "Delete Video Files")

                {
                    (int _statusCode, var response) = await _apiService.DeleteAttachments(body);
                }

                videoFilesSearchBar.Text = "";

                VideoFileActionPicker.SelectedIndex = 0;

                getUploads();

                uncheckVideoFileCheckBoxes();

                updateVideoFilesActionButton();

                while (SelectedVideoFiles.Count() > 0)
                {
                    SelectedVideoFiles.RemoveAt(0);
                }
            }
        }
    }

    public void refreshVideoFilesButtonClicked(object sender, EventArgs e)
    {
        videoFilesSearchBar.Text = "";

        getUploads();

        updateVideoFilesActionButton();
    }


    private void VideoFileActionPickerOnSelectedIndexChanged(object sender, EventArgs e)
    {
        updateVideoFilesActionButton();
    }

    public void selectAllVideoFilesButtonClicked(object sender, EventArgs e)
    {
        Button selectAllFolders = (Button)sender;

        toggleVideoFilesCheckBoxes();

        updateVideoFilesActionButton();
    }
    private void PopulateVideoFilesActionPicker()
    {
        VideoFileActionPickerOptions = new List<PickerOption>
            {
                new PickerOption { ID = "0", Name = "" },
                new PickerOption { ID = "1", Name = "Delete Video Files" },
            };

        foreach (var pickerOption in VideoFileActionPickerOptions)
        {
            VideoFileActionPicker.Items.Add(pickerOption.Name);
        }
    }

    public void uncheckVideoFileCheckBoxes()
    {
        var rootViewsAndTheirDescendants = videoFilesCollectionView.GetVisualTreeDescendants();

        foreach (VisualElement element in rootViewsAndTheirDescendants)
        {
            if (element is Microsoft.Maui.Controls.CheckBox)
            {
                CheckBox checkbox = (CheckBox)element;
                checkbox.IsChecked = false;
            }
        }
    }

    public void toggleVideoFilesCheckBoxes()
    {
        var rootViewsAndTheirDescendants = videoFilesCollectionView.GetVisualTreeDescendants();

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

    public void videoFileCheckedChanged(object sender, EventArgs e)
    {
        CheckBox selectFolder = (CheckBox)sender;

        updateVideoFilesActionButton();
    }

    public async void updateVideoFilesActionButton()
    {
        var rootViewsAndTheirDescendants = videoFilesCollectionView.GetVisualTreeDescendants();

        int videoFilesCount = 0;
        foreach (VisualElement element in rootViewsAndTheirDescendants)
        {
            if (element is Microsoft.Maui.Controls.CheckBox)
            {
                CheckBox checkbox = (CheckBox)element;
                if (checkbox.IsChecked)
                {
                    videoFilesCount++;
                }
            }
        }

        if (videoFilesCount > 0)
        {
            string videoFilesLabel = videoFilesCount == 1 ? "Video File" : "Video Files";
            selectedVideoFilesCountLabel.Text = Convert.ToString(videoFilesCount) + " " + videoFilesLabel + " selected";
        }
        else
        {
            selectedVideoFilesCountLabel.Text = "No Video Files selected";
        }

        PickerOption selectedOption = null;
        foreach (var pickerOption in VideoFileActionPickerOptions)
        {
            if (Convert.ToInt32(pickerOption.ID) == VideoFileActionPicker.SelectedIndex)
            {
                selectedOption = pickerOption;
                break;
            }
        }

        if (videoFilesCount > 0 && (selectedOption != null && selectedOption.Name != ""))
        {
            updateVideoFilesButton.BackgroundColor = Colors.Orange;
            updateVideoFilesButton.TextColor = Colors.Black;
            updateVideoFilesButtonImage.Color = Colors.White;
            VideoFileActionPicker.TextColor = Colors.FloralWhite;
        }
        else
        {
            updateVideoFilesButton.BackgroundColor = Colors.Black;
            updateVideoFilesButton.TextColor = Colors.Gray;
            updateVideoFilesButtonImage.Color = Colors.Gray;
            VideoFileActionPicker.TextColor = Colors.Gray;
        }
    }

    public async void videoFilesSearchInputTextChanged(object sender, EventArgs e)
    {
        SearchBar searchBar = (SearchBar)sender;
        videoFilesCollectionView.ItemsSource = VideoFiles.Where(videoFile =>
            videoFile.fileName.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)
        );

        VideoFileActionPicker.SelectedIndex = 0;

        uncheckVideoFileCheckBoxes();

        updateVideoFilesActionButton();
    }

    public async void addVideoFileToSelectedItems(VideoFile videoFile)
    {
        SelectedVideoFiles.Add(videoFile);
        GridMediaProcessing.IsVisible = videoFile.aasmState == "created";
    }

    public async void previousLinkVideoFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        VideoFile videoFile = (VideoFile)button.BindingContext;
        GridMediaPlayer.IsVisible = false;
        mediaElement.Stop();
        mediaElement.Source = null;

        while (SelectedVideoFiles.Count() > 0)
        {
            SelectedVideoFiles.RemoveAt(0);
        }

        try
        {
            VideoFile nextVideoFile = VideoFiles[VideoFiles.IndexOf(videoFile) - 1];

            foreach (VideoFile _videoFile in VideoFiles)
            {
                if (_videoFile.id == nextVideoFile.id)
                {
                    addVideoFileToSelectedItems(_videoFile);
                    break;
                }
            }
        }
        catch
        {

        }
    }

    public async void nextLinkVideoFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        VideoFile videoFile = (VideoFile)button.BindingContext;
        GridMediaPlayer.IsVisible = false;
        mediaElement.Stop();
        mediaElement.Source = null;

        while (SelectedVideoFiles.Count() > 0)
        {
            SelectedVideoFiles.RemoveAt(0);
        }

        try
        {
            VideoFile nextVideoFile = VideoFiles[VideoFiles.IndexOf(videoFile) + 1];

            foreach (VideoFile _videoFile in VideoFiles)
            {
                if (_videoFile.id == nextVideoFile.id)
                {
                    addVideoFileToSelectedItems(_videoFile);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            //
        }
    }

    public async void removeVideoFileFromSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        VideoFile videoFile = (VideoFile)button.BindingContext;

        GridMediaPlayer.IsVisible = false;

        mediaElement.Stop();
        mediaElement.Source = null;

        int i = 0;
        foreach (VideoFile _videoFile in SelectedVideoFiles)
        {
            if (_videoFile.id == videoFile.id)
            {
                SelectedVideoFiles.RemoveAt(i);
                break;
            }
            i += 1;
        }
    }

    public async void addVideoFileToSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        VideoFile videoFile = (VideoFile)button.BindingContext;

        while (SelectedVideoFiles.Count() > 0)
        {
            SelectedVideoFiles.RemoveAt(0);
        }

        addVideoFileToSelectedItems(videoFile);

    }

    public async void SelectedVideoFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        VideoFile _selectedVideoFile = e.CurrentSelection.FirstOrDefault() as VideoFile;
    }

    public async void VideoFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        VideoFile _selectedVideoFile = e.CurrentSelection.FirstOrDefault() as VideoFile;
    }

    public async void playVideoFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        VideoFile videoFile = (VideoFile)button.BindingContext;

        try
        {
            for (int i = 0; i < 10; i++)
            {
                string videoFileId = Convert.ToString(videoFile.id);

                (int _statusCode, var response) = await _apiService.getVideoStream(videoFileId);

                VideoStreamResponse jsonResponse = JsonSerializer.Deserialize<VideoStreamResponse>(response);

                if (jsonResponse != null)
                {
                    if (jsonResponse.m3u8Exists)
                    {
                        VideoStreams.Add(jsonResponse);
                        break;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //
        }

        bool found = false;
        try
        {
            foreach (VideoStreamResponse videoStream in VideoStreams)
            {
                if (videoStream.id == videoFile.id)
                {
                    found = true;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            //
        }

        if (found)
        {
            GridMediaPlayer.IsVisible = true;
            GridMediaProcessing.IsVisible = false;
            string id = Convert.ToString(videoFile.id);
            //mediaElement.Source = new Uri("http://192.168.1.11:3001/playlists/video-" + id + ".m3u8");
            mediaElement.Source = new Uri("https://link12.ddns.net:5050/playlists/video-" + id + ".m3u8");
            mediaElement.Play();
        }
    }

    public void SetTimeout(Action action, int ms)
    {
        Task.Delay(ms).ContinueWith((task) =>
        {
            action();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public string getVideoFileURL(VideoFile videoFile)
    {
        string url = "https://link12.ddns.net/" +
            videoFile.folder.dataUrl +
            "/video/" + videoFile.dataUrl;
        return url;
    }

    private async void VideoFileOpenWebURL_Clicked(object sender, EventArgs e)
    {
        try
        {
            Button button = (Button)sender;
            VideoFile videoFile = (VideoFile)button.BindingContext;
            string url = getVideoFileURL(videoFile);
            await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(url);
        }
        catch (Exception _ex)
        {
            //
        }
    }

    private async void VideoFileSetClipboardButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            Button button = (Button)sender;
            VideoFile videoFile = (VideoFile)button.BindingContext;
            string url = getVideoFileURL(videoFile);
            await Clipboard.Default.SetTextAsync(url);
        }
        catch (Exception _ex)
        {
            //
        }

        VideoFileActionLabel.Text = "Video File URL copied to clipboard";

        SetTimeout(() =>
        {
            VideoFileActionLabel.Text = "";
        }, 2000);
    }





    public List<string> getSelectedAudioFileIds()
    {
        List<string> ids = new List<string>();
        var rootViewsAndTheirDescendants = audioFilesCollectionView.GetVisualTreeDescendants();
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

    public async void updateAudioFilesButtonClicked(object sender, EventArgs e)
    {
        List<string> ids = getSelectedImageFileIds();
        List<string> AudioFileNames = new List<string>();
        foreach (AudioFile audioFile in AudioFiles)
        {
            if (ids.Contains(Convert.ToString(audioFile.id)))
            {
                AudioFileNames.Add(audioFile.fileName);
            }
        }

        PickerOption selectedOption = null;
        foreach (var pickerOption in AudioFileActionPickerOptions)
        {
            if (Convert.ToInt32(pickerOption.ID) == AudioFileActionPicker.SelectedIndex)
            {
                selectedOption = pickerOption;
                break;
            }
        }

        if (selectedOption != null)
        {
            string action = await DisplayActionSheet(selectedOption.Name, "Cancel", "Delete", String.Join("\n", AudioFileNames));

            if (action == "Delete")
            {

                var audioFileIds = new CollectionIds
                {
                    id = ids.ToArray(),
                    type = ["AudioFile"]
                };

                byte[] body = JsonSerializer.SerializeToUtf8Bytes(audioFileIds);

                if (selectedOption.Name == "Delete Audio Files")

                {
                    (int _statusCode, var response) = await _apiService.DeleteAttachments(body);
                }

                audioFilesSearchBar.Text = "";

                AudioFileActionPicker.SelectedIndex = 0;

                getUploads();

                uncheckAudioFileCheckBoxes();

                updateAudioFilesActionButton();

                while (SelectedAudioFiles.Count() > 0)
                {
                    SelectedAudioFiles.RemoveAt(0);
                }
            }
        }
    }

    public void refreshAudioFilesButtonClicked(object sender, EventArgs e)
    {
        audioFilesSearchBar.Text = "";

        getUploads();

        updateAudioFilesActionButton();
    }


    private void AudioFileActionPickerOnSelectedIndexChanged(object sender, EventArgs e)
    {
        updateAudioFilesActionButton();
    }

    public void selectAllAudioFilesButtonClicked(object sender, EventArgs e)
    {
        Button selectAllFolders = (Button)sender;

        toggleAudioFilesCheckBoxes();

        updateAudioFilesActionButton();
    }
    private void PopulateAudioFilesActionPicker()
    {
        AudioFileActionPickerOptions = new List<PickerOption>
            {
                new PickerOption { ID = "0", Name = "" },
                new PickerOption { ID = "1", Name = "Delete Image Files" },
            };

        foreach (var pickerOption in AudioFileActionPickerOptions)
        {
            AudioFileActionPicker.Items.Add(pickerOption.Name);
        }
    }

    public void uncheckAudioFileCheckBoxes()
    {
        var rootViewsAndTheirDescendants = imageFilesCollectionView.GetVisualTreeDescendants();

        foreach (VisualElement element in rootViewsAndTheirDescendants)
        {
            if (element is Microsoft.Maui.Controls.CheckBox)
            {
                CheckBox checkbox = (CheckBox)element;
                checkbox.IsChecked = false;
            }
        }
    }

    public void toggleAudioFilesCheckBoxes()
    {
        var rootViewsAndTheirDescendants = audioFilesCollectionView.GetVisualTreeDescendants();

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

    public void audioFileCheckedChanged(object sender, EventArgs e)
    {
        CheckBox selectFolder = (CheckBox)sender;

        updateAudioFilesActionButton();
    }

    public async void updateAudioFilesActionButton()
    {
        var rootViewsAndTheirDescendants = audioFilesCollectionView.GetVisualTreeDescendants();

        int audioFilesCount = 0;
        foreach (VisualElement element in rootViewsAndTheirDescendants)
        {
            if (element is Microsoft.Maui.Controls.CheckBox)
            {
                CheckBox checkbox = (CheckBox)element;
                if (checkbox.IsChecked)
                {
                    audioFilesCount++;
                }
            }
        }

        if (audioFilesCount > 0)
        {
            string audioFilesLabel = audioFilesCount == 1 ? "Audio File" : "Audio Files";
            selectedAudioFilesCountLabel.Text = Convert.ToString(audioFilesCount) + " " + audioFilesLabel + " selected";
        }
        else
        {
            selectedImageFilesCountLabel.Text = "No Image Files selected";
        }

        PickerOption selectedOption = null;
        foreach (var pickerOption in AudioFileActionPickerOptions)
        {
            if (Convert.ToInt32(pickerOption.ID) == AudioFileActionPicker.SelectedIndex)
            {
                selectedOption = pickerOption;
                break;
            }
        }

        if (audioFilesCount > 0 && (selectedOption != null && selectedOption.Name != ""))
        {
            updateAudioFilesButton.BackgroundColor = Colors.Orange;
            updateAudioFilesButton.TextColor = Colors.Black;
            updateAudioFilesButtonImage.Color = Colors.White;
            AudioFileActionPicker.TextColor = Colors.FloralWhite;
        }
        else
        {
            updateAudioFilesButton.BackgroundColor = Colors.Black;
            updateAudioFilesButton.TextColor = Colors.Gray;
            updateAudioFilesButtonImage.Color = Colors.Gray;
            AudioFileActionPicker.TextColor = Colors.Gray;
        }
    }

    public async void audioFilesSearchInputTextChanged(object sender, EventArgs e)
    {
        SearchBar searchBar = (SearchBar)sender;
        audioFilesCollectionView.ItemsSource = AudioFiles.Where(audioFile =>
            audioFile.fileName.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)
        );

        AudioFileActionPicker.SelectedIndex = 0;

        uncheckAudioFileCheckBoxes();

        updateAudioFilesActionButton();
    }

    public async void addAudioFileToSelectedItems(AudioFile audioFile)
    {
        SelectedAudioFiles.Add(audioFile);
        GridMediaProcessing.IsVisible = audioFile.aasmState == "created";
    }

    public async void previousLinkAudioFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        AudioFile audioFile = (AudioFile)button.BindingContext;
        GridMediaPlayer.IsVisible = false;
        mediaElement.Stop();
        mediaElement.Source = null;

        while (SelectedAudioFiles.Count() > 0)
        {
            SelectedAudioFiles.RemoveAt(0);
        }

        try
        {
            AudioFile nextAudioFile = AudioFiles[AudioFiles.IndexOf(audioFile) - 1];

            foreach (AudioFile _audioFile in AudioFiles)
            {
                if (_audioFile.id == nextAudioFile.id)
                {
                    addAudioFileToSelectedItems(_audioFile);
                    break;
                }
            }
        }
        catch
        {

        }
    }

    public async void nextLinkAudioFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        AudioFile audioFile = (AudioFile)button.BindingContext;
        GridMediaPlayer.IsVisible = false;
        mediaElement.Stop();
        mediaElement.Source = null;

        while (SelectedAudioFiles.Count() > 0)
        {
            SelectedAudioFiles.RemoveAt(0);
        }

        try
        {
            AudioFile nextAudioFile = AudioFiles[AudioFiles.IndexOf(audioFile) + 1];

            foreach (AudioFile _audioFile in AudioFiles)
            {
                if (_audioFile.id == nextAudioFile.id)
                {
                    addAudioFileToSelectedItems(_audioFile);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            //
        }
    }

    public async void removeAudioFileFromSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        AudioFile audioFile = (AudioFile)button.BindingContext;

        GridMediaPlayer.IsVisible = false;

        mediaElement.Stop();
        mediaElement.Source = null;

        int i = 0;
        foreach (AudioFile _audioFile in SelectedAudioFiles)
        {
            if (_audioFile.id == audioFile.id)
            {
                SelectedAudioFiles.RemoveAt(i);
                break;
            }
            i += 1;
        }
    }

    public async void addAudioFileToSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        AudioFile audioFile = (AudioFile)button.BindingContext;

        while (SelectedAudioFiles.Count() > 0)
        {
            SelectedAudioFiles.RemoveAt(0);
        }

        addAudioFileToSelectedItems(audioFile);
    }

    public async void SelectedAudioFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AudioFile _selectedAudioFile = e.CurrentSelection.FirstOrDefault() as AudioFile;
    }

    public async void AudioFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AudioFile _selectedAudioFile = e.CurrentSelection.FirstOrDefault() as AudioFile;
    }

    public async void playAudioFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        AudioFile audioFile = (AudioFile)button.BindingContext;

        try
        {
            for (int i = 0; i < 10; i++)
            {
                string audioFileId = Convert.ToString(audioFile.id);

                (int _statusCode, var response) = await _apiService.getAudioStream(audioFileId);

                AudioStreamResponse jsonResponse = JsonSerializer.Deserialize<AudioStreamResponse>(response);

                if (jsonResponse != null)
                {
                    if (jsonResponse.m3u8Exists)
                    {
                        AudioStreams.Add(jsonResponse);
                        break;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //
        }

        bool found = false;
        try
        {
            foreach (AudioStreamResponse audioStream in AudioStreams)
            {
                if (audioStream.id == audioFile.id)
                {
                    found = true;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            //
        }

        if (found)
        {
            GridMediaPlayer.IsVisible = true;
            GridMediaProcessing.IsVisible = false;
            string id = Convert.ToString(audioFile.id);
            //mediaElement.Source = new Uri("http://192.168.1.11:3001/playlists/audio-" + id + ".m3u8");
            mediaElement.Source = new Uri("https://link12.ddns.net:5050/playlists/audio-" + id + ".m3u8");
            mediaElement.Play();
        }
    }

    public string getAudioFileURL(AudioFile audioFile)
    {
        string url = "https://link12.ddns.net/" +
            audioFile.folder.dataUrl +
            "/audio/" + audioFile.dataUrl;
        return url;
    }

    private async void AudioFileOpenWebURL_Clicked(object sender, EventArgs e)
    {
        try
        {
            Button button = (Button)sender;
            AudioFile audioFile = (AudioFile)button.BindingContext;
            string url = getAudioFileURL(audioFile);
            await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(url);
        }
        catch (Exception _ex)
        {
            //
        }
    }

    private async void AudioFileSetClipboardButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            Button button = (Button)sender;
            AudioFile audioFile = (AudioFile)button.BindingContext;
            string url = getAudioFileURL(audioFile);
            await Clipboard.Default.SetTextAsync(url);
        }
        catch (Exception _ex)
        {
            //
        }

        AudioFileActionLabel.Text = "Audio File URL copied to clipboard";

        SetTimeout(() =>
        {
            AudioFileActionLabel.Text = "";
        }, 2000);
    }

    public async void pdfFilesSearchInputTextChanged(object sender, EventArgs e)
    {
        SearchBar searchBar = (SearchBar)sender;
        pdfFilesCollectionView.ItemsSource = PdfFiles.Where(pdfFile =>
            pdfFile.fileName.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)
        );
    }

    public async void addPdfFileToSelectedItems(PdfFile pdfFile)
    {
        SelectedPdfFiles.Add(pdfFile);
    }

    public async void previousLinkPdfFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        PdfFile pdfFile = (PdfFile)button.BindingContext;

        while (SelectedPdfFiles.Count() > 0)
        {
            SelectedPdfFiles.RemoveAt(0);
        }

        try
        {
            PdfFile nextPdfFile = PdfFiles[PdfFiles.IndexOf(pdfFile) - 1];

            foreach (PdfFile _pdfFile in PdfFiles)
            {
                if (_pdfFile.id == nextPdfFile.id)
                {
                    addPdfFileToSelectedItems(_pdfFile);
                    break;
                }
            }
        }
        catch
        {

        }
    }

    public async void nextLinkPdfFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        PdfFile pdfFile = (PdfFile)button.BindingContext;

        while (SelectedPdfFiles.Count() > 0)
        {
            SelectedPdfFiles.RemoveAt(0);
        }

        try
        {
            PdfFile nextPdfFile = PdfFiles[PdfFiles.IndexOf(pdfFile) + 1];

            foreach (PdfFile _pdfFile in PdfFiles)
            {
                if (_pdfFile.id == nextPdfFile.id)
                {
                    addPdfFileToSelectedItems(_pdfFile);
                    break;
                }
            }
        }
        catch
        {

        }
    }

    public async void removePdfFileFromSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        PdfFile pdfFile = (PdfFile)button.BindingContext;
        int i = 0;
        foreach (PdfFile _pdfFile in SelectedPdfFiles)
        {
            if (_pdfFile.id == pdfFile.id)
            {
                SelectedPdfFiles.RemoveAt(i);
                break;
            }
            i += 1;
        }
    }

    public async void addPdfFileToSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        PdfFile pdfFile = (PdfFile)button.BindingContext;

        while (SelectedPdfFiles.Count() > 0)
        {
            SelectedPdfFiles.RemoveAt(0);
        }

        addPdfFileToSelectedItems(pdfFile);
    }

    public async void SelectedPdfFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PdfFile _selectedPdfFile = e.CurrentSelection.FirstOrDefault() as PdfFile;
    }

    public async void PdfFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PdfFile _selectedPdfFile = e.CurrentSelection.FirstOrDefault() as PdfFile;
    }

    public async void textFilesSearchInputTextChanged(object sender, EventArgs e)
    {
        SearchBar searchBar = (SearchBar)sender;
        textFilesCollectionView.ItemsSource = TextFiles.Where(textFile =>
            textFile.fileName.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)
        );
    }

    public async void addTextFileToSelectedItems(TextFile textFile)
    {
        SelectedTextFiles.Add(textFile);
    }

    public async void previousLinkTextFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        TextFile textFile = (TextFile)button.BindingContext;

        while (SelectedTextFiles.Count() > 0)
        {
            SelectedTextFiles.RemoveAt(0);
        }

        try
        {
            TextFile nextTextFile = TextFiles[TextFiles.IndexOf(textFile) - 1];

            foreach (TextFile _textFile in TextFiles)
            {
                if (_textFile.id == nextTextFile.id)
                {
                    addTextFileToSelectedItems(_textFile);
                    break;
                }
            }
        }
        catch
        {

        }
    }

    public async void nextLinkTextFileClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        TextFile textFile = (TextFile)button.BindingContext;

        while (SelectedTextFiles.Count() > 0)
        {
            SelectedTextFiles.RemoveAt(0);
        }

        try
        {
            TextFile nextTextFile = TextFiles[TextFiles.IndexOf(textFile) + 1];

            foreach (TextFile _textFile in TextFiles)
            {
                if (_textFile.id == nextTextFile.id)
                {
                    addTextFileToSelectedItems(_textFile);
                    break;
                }
            }
        }
        catch
        {

        }
    }

    public async void removeTextFileFromSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        TextFile textFile = (TextFile)button.BindingContext;
        int i = 0;
        foreach (TextFile _textFile in SelectedTextFiles)
        {
            if (_textFile.id == textFile.id)
            {
                SelectedTextFiles.RemoveAt(i);
                break;
            }
            i += 1;
        }
    }

    public async void addTextFileToSelection(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        TextFile textFile = (TextFile)button.BindingContext;

        while (SelectedTextFiles.Count() > 0)
        {
            SelectedTextFiles.RemoveAt(0);
        }

        addTextFileToSelectedItems(textFile);
    }

    public async void SelectedTextFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TextFile _selectedTextFile = e.CurrentSelection.FirstOrDefault() as TextFile;
    }

    public async void TextFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TextFile _selectedTextFile = e.CurrentSelection.FirstOrDefault() as TextFile;
    }

    public void DisplayPageUnloaded(object? sender, EventArgs e)
    {
        // Stop and cleanup MediaElement when we navigate away
        mediaElement.Handler?.DisconnectHandler();
    }

    public void DisplayPageLoaded(object? sender, EventArgs e)
    {
        //
    }
}
