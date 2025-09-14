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
public partial class DisplayItemPage : ContentPage
{
    private readonly IApiService _apiService;

    private readonly AppShellViewModel _appShellViewModel;

    private Folder _folder;
    public ObservableCollection<ImageFile> ImageFiles { get; set; }
    public DisplayItemPage(IApiService apiService, AppShellViewModel appShellViewModel, Folder folder)
    {
        _apiService = apiService;
        _appShellViewModel = appShellViewModel;
        _folder = folder;
        ImageFiles = new ObservableCollection<ImageFile> { };
        InitializeComponent();
        BindingContext = this;
        folderName.Text = folder.name;
        getUploads();
    }
    public async void getUploads()
    {
        string folderId = Convert.ToString(_folder.id);

        var str = JsonSerializer.SerializeToUtf8Bytes("," + folderId);

        string encodedFolderId = Convert.ToBase64String(str);

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

        imageFilesCount.Text = Convert.ToString(ImageFiles.Count()) + " Images";
    }
}
