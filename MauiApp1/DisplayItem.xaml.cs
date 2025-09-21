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

    public ObservableCollection<ImageFile> SelectedImageFiles { get; set; }

    public DisplayItemPage(IApiService apiService, AppShellViewModel appShellViewModel, Folder folder)
    {
        _apiService = apiService;
        _appShellViewModel = appShellViewModel;
        _folder = folder;
        ImageFiles = new ObservableCollection<ImageFile> { };
        SelectedImageFiles = new ObservableCollection<ImageFile> { };
        InitializeComponent();
        BindingContext = this;
        folderName.Text = folder.name;
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

        imageFilesCount.Text = Convert.ToString(ImageFiles.Count()) + " Images";
    }

    public async void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        Console.WriteLine($"ScrollX: {e.ScrollX}, ScrollY: {e.ScrollY}");
    }

    public async void ImageFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ImageFile selectedImageFile = e.CurrentSelection.FirstOrDefault() as ImageFile;

        SelectedImageFiles.Add(selectedImageFile);

        var ids = SelectedImageFiles.Select(x => x.id);

    }
}
