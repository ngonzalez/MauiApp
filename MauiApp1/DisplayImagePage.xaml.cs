using MauiApp1.Platforms.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;
using Windows.Services.Maps;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MauiApp1;

public partial class DisplayImagePage : ContentPage
{
    private readonly IApiService _apiService;

    private readonly AppShellViewModel _appShellViewModel;

    private ImageFile _imageFile;

    public DisplayImagePage(IApiService apiService, AppShellViewModel appShellViewModel, ImageFile imageFile)
    {
        _apiService = apiService;
        _appShellViewModel = appShellViewModel;
        _imageFile = imageFile;
        InitializeComponent();
        BindingContext = this;
        imageFileUrl.Text = imageFile.fileUrl;
    }

}
