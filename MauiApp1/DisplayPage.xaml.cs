using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using MauiApp1.Platforms.Windows;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;
using Windows.ApplicationModel.Store;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Services.Maps;
using Windows.System.UserProfile;
using static Microsoft.Maui.ApplicationModel.Permissions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MauiApp1;

public partial class DisplayPage : ContentPage
{
    private readonly IApiService _apiService;

    private readonly AppShellViewModel _appShellViewModel;

    private Folder _folder;

    public ObservableCollection<ImageFile> ImageFiles { get; set; }

    public ObservableCollection<ImageFile> SelectedImageFiles { get; set; }

    public ObservableCollection<VideoFile> VideoFiles { get; set; }

    public ObservableCollection<VideoFile> SelectedVideoFiles { get; set; }

    public DisplayPage(IApiService apiService, AppShellViewModel appShellViewModel, Folder folder)
    {
        _apiService = apiService;
        _appShellViewModel = appShellViewModel;
        _folder = folder;

        // ImageFile collection
        ImageFiles = new ObservableCollection<ImageFile> { };
        SelectedImageFiles = new ObservableCollection<ImageFile> { };

        // VideoFile collection
        VideoFiles = new ObservableCollection<VideoFile> { };
        SelectedVideoFiles = new ObservableCollection<VideoFile> { };

        InitializeComponent();
        BindingContext = this;

        // Set folder name
        folderName1.Text = folder.name;
        folderName2.Text = folder.name;

        // Set default visibility for grids
        GridImageFiles.IsVisible = false;
        GridImageFilesDetails.IsVisible = false;
        GridVideoFiles.IsVisible = false;
        GridVideoFilesDetails.IsVisible = false;
        GridMediaPlayer.IsVisible = false;

        // Get media files from backend
        getUploads();
    }

    public async void getUploads()
    {
        string folderId = Convert.ToString(_folder.id);
        var folderIdsUtf8 = JsonSerializer.SerializeToUtf8Bytes("," + folderId);
        string encodedFolderId = Convert.ToBase64String(folderIdsUtf8);
        var response = await _apiService.GetAllUploads("?folderIds=" + encodedFolderId);
        var uploadsResponse = JsonSerializer.Deserialize<Upload[]>(response);

        while (ImageFiles.Count() > 0)
        {
            ImageFiles.RemoveAt(0);
        }

        foreach (Upload upload in uploadsResponse)
        {
            if (upload.imageFiles.Length > 0)
            {
                foreach(ImageFile imageFile in upload.imageFiles)
                {
                    ImageFiles.Add(imageFile);
                }
            }
        }

        string imageFileLabel = ImageFiles.Count() > 1 ? "Image Files" : "Image File";
        imageFilesCount.Text = Convert.ToString(ImageFiles.Count()) + " " + imageFileLabel;
        GridImageFiles.IsVisible = ImageFiles.Count() > 0;
        GridImageFilesDetails.IsVisible = ImageFiles.Count() > 0;

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

        string videoFileLabel = VideoFiles.Count() > 1 ? "Video Files" : "Video File";
        videoFilesCount.Text = Convert.ToString(VideoFiles.Count()) + " " + videoFileLabel;
        GridVideoFiles.IsVisible = VideoFiles.Count() > 0;
        GridVideoFilesDetails.IsVisible = VideoFiles.Count() > 0;
    }

    public async void openNewWindowImageFile(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        ImageFile imageFile = (ImageFile)button.BindingContext;
        Window secondWindow = new Window(new ShowImageFilePage(_apiService, _appShellViewModel, imageFile));
        App.Current.OpenWindow(secondWindow);
    }

    public async void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        Console.WriteLine($"ScrollX: {e.ScrollX}, ScrollY: {e.ScrollY}");
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
                    SelectedImageFiles.Add(_imageFile);
                    break;
                }
            }
        } catch
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
                    SelectedImageFiles.Add(_imageFile);
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

        SelectedImageFiles.Add(imageFile);
    }

    public async void SelectedImageFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ImageFile _selectedImageFile = e.CurrentSelection.FirstOrDefault() as ImageFile;
    }

    public async void ImageFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ImageFile _selectedImageFile = e.CurrentSelection.FirstOrDefault() as ImageFile;
    }

    public async void openNewWindowVideoFile(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        VideoFile videoFile = (VideoFile)button.BindingContext;
        GridMediaPlayer.IsVisible = false;
        mediaElement.Stop();
        mediaElement.Source = null;
        Window secondWindow = new Window(new ShowVideoFilePage(_apiService, _appShellViewModel, videoFile));
        App.Current.OpenWindow(secondWindow);
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
                    SelectedVideoFiles.Add(_videoFile);
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
                    SelectedVideoFiles.Add(_videoFile);
                    break;
                }
            }
        }
        catch
        {

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

        SelectedVideoFiles.Add(videoFile);
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
        GridMediaPlayer.IsVisible = true;
        mediaElement.Source = new Uri("https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4");
        mediaElement.Play();
    }
    public void DisplayPageUnloaded(object? sender, EventArgs e)
    {
        // Stop and cleanup MediaElement when we navigate away
        mediaElement.Handler?.DisconnectHandler();
    }
}
