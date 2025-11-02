namespace MauiApp1;

public partial class ShowImageFilePage : ContentPage
{
    private readonly IApiService _apiService;

    private readonly AppShellViewModel _appShellViewModel;

    private ImageFile _imageFile;
    public ShowImageFilePage(IApiService apiService, AppShellViewModel appShellViewModel, ImageFile imageFile)
    {
        _apiService = apiService;
        _appShellViewModel = appShellViewModel;
        _imageFile = imageFile;
        InitializeComponent();
        BindingContext = this;
        mainImage.Uri = new System.Uri(imageFile.fileUrl);
    }
}
