namespace MauiApp1;

public partial class ShowVideoFilePage : ContentPage
{
    private readonly IApiService _apiService;

    private readonly AppShellViewModel _appShellViewModel;

    private VideoFile _videoFile;
    public ShowVideoFilePage(IApiService apiService, AppShellViewModel appShellViewModel, VideoFile videoFile)
    {
        _apiService = apiService;
        _appShellViewModel = appShellViewModel;
        _videoFile = videoFile;
        InitializeComponent();
        BindingContext = this;
    }
}
