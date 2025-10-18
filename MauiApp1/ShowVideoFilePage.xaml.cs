using MauiApp1.Platforms.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;
using Windows.Services.Maps;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
