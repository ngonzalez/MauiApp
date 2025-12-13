using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace MauiApp1;

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
    private readonly IApiService _apiService;

    private readonly AppShellViewModel _appShellViewModel;

    private Folder _folder;

    public ObservableCollection<ImageFile> ImageFiles { get; set; }
    public ObservableCollection<ImageFile> SelectedImageFiles { get; set; }

    public ObservableCollection<VideoFile> VideoFiles { get; set; }
    public ObservableCollection<VideoFile> SelectedVideoFiles { get; set; }
    public ObservableCollection<VideoStreamResponse> VideoStreams { get; set; }

    public ObservableCollection<AudioFile> AudioFiles { get; set; }
    public ObservableCollection<AudioFile> SelectedAudioFiles { get; set; }
    public ObservableCollection<AudioStreamResponse> AudioStreams { get; set; }

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

        // VideoStreams collection
        VideoStreams = new ObservableCollection<VideoStreamResponse> { };

        // AudioFile collection
        AudioFiles = new ObservableCollection<AudioFile> { };
        SelectedAudioFiles = new ObservableCollection<AudioFile> { };

        // AudioStreams collection
        AudioStreams = new ObservableCollection<AudioStreamResponse> { };

        InitializeComponent();
        BindingContext = this;

        // Set folder name
        folderName1.Text = folder.name;
        folderName2.Text = folder.name;
        folderName3.Text = folder.name;

        // Set default visibility for grids
        GridImageFiles.IsVisible = false;
        GridImageFilesDetails.IsVisible = false;
        GridVideoFiles.IsVisible = false;
        GridVideoFilesDetails.IsVisible = false;
        GridAudioFiles.IsVisible = false;
        GridAudioFilesDetails.IsVisible = false;
        GridMediaPlayer.IsVisible = false;
        GridMediaProcessing.IsVisible = false;

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

        string imageFileLabel = ImageFiles.Count() > 1 ? "Image Files" : "Image File";
        imageFilesCount.Text = Convert.ToString(ImageFiles.Count()) + " " + imageFileLabel;
        GridImageFiles.IsVisible = ImageFiles.Count() > 0;
        GridImageFilesDetails.IsVisible = ImageFiles.Count() > 0;

        // VideoFile
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

        // AudioFile
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

        string audioFileLabel = AudioFiles.Count() > 1 ? "Audio Files" : "Audio File";
        audioFilesCount.Text = Convert.ToString(AudioFiles.Count()) + " " + audioFileLabel;
        GridAudioFiles.IsVisible = AudioFiles.Count() > 0;
        GridAudioFilesDetails.IsVisible = AudioFiles.Count() > 0;
    }

    public async void imageFilesSearchInputTextChanged(object sender, EventArgs e)
    {
        SearchBar searchBar = (SearchBar)sender;
        imageFilesCollectionView.ItemsSource = ImageFiles.Where(imageFile =>
            imageFile.fileName.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)
        );
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

    public async void videoFilesSearchInputTextChanged(object sender, EventArgs e)
    {
        SearchBar searchBar = (SearchBar)sender;
        videoFilesCollectionView.ItemsSource = VideoFiles.Where(videoFile =>
            videoFile.fileName.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)
        );
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

        try
        {
            for (int i = 0; i < 10; i++)
            {
                string videoFileId = Convert.ToString(videoFile.id);

                (int _statusCode, var response) = await _apiService.getVideoStream(videoFileId);

                VideoStreamResponse jsonResponse = JsonSerializer.Deserialize<VideoStreamResponse>(response);

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
            string id = Convert.ToString(videoFile.id);
            //mediaElement.Source = new Uri("http://192.168.1.11:3001/playlists/video-" + id + ".m3u8");
            mediaElement.Source = new Uri("https://link12.ddns.net:5050/playlists/video-" + id + ".m3u8");
            mediaElement.Play();
        }
        else
        {
            GridMediaProcessing.IsVisible = false;
        }
    }

    public async void audioFilesSearchInputTextChanged(object sender, EventArgs e)
    {
        SearchBar searchBar = (SearchBar)sender;
        audioFilesCollectionView.ItemsSource = AudioFiles.Where(audioFile =>
            audioFile.fileName.Contains(searchBar.Text, StringComparison.OrdinalIgnoreCase)
        );
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
                    SelectedAudioFiles.Add(_audioFile);
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
                    SelectedAudioFiles.Add(_audioFile);
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

        SelectedAudioFiles.Add(audioFile);
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
            string id = Convert.ToString(audioFile.id);
            //mediaElement.Source = new Uri("http://192.168.1.11:3001/playlists/audio-" + id + ".m3u8");
            mediaElement.Source = new Uri("https://link12.ddns.net:5050/playlists/audio-" + id + ".m3u8");
            mediaElement.Play();
        }
        else
        {
            GridMediaProcessing.IsVisible = false;
        }
    }

    public void DisplayPageUnloaded(object? sender, EventArgs e)
    {
        // Stop and cleanup MediaElement when we navigate away
        mediaElement.Handler?.DisconnectHandler();
    }
}
