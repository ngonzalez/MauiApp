using MauiApp1.Platforms.Windows;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;

namespace MauiApp1;
public partial class DisplayItemPage : ContentPage
{
    private Folder _folder;
    public DisplayItemPage(Folder folder)
	{
        _folder = folder;
        InitializeComponent();
        folderName.Text = folder.name;
    }
}
