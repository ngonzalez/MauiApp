namespace MauiApp1;

public partial class ShowAudioFilePage : ContentPage
{
    private readonly IApiService _apiService;

    private readonly AppShellViewModel _appShellViewModel;

    private AudioFile _audioFile;
    public ShowAudioFilePage(IApiService apiService, AppShellViewModel appShellViewModel, AudioFile audioFile)
    {
        _apiService = apiService;
        _appShellViewModel = appShellViewModel;
        _audioFile = audioFile;
        InitializeComponent();
        BindingContext = this;
    }
}
