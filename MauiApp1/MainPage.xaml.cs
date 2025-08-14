using System;
using System.Collections.ObjectModel;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        public class Item
        {
            public string Name { get; set; }
        }
        public ObservableCollection<Item> Items { get; set; }

        private readonly IFolderPicker _folderPicker;

        public MainPage(IFolderPicker folderPicker)
        {
            InitializeComponent();
            _folderPicker = folderPicker;
            Items = new ObservableCollection<Item>{};
            BindingContext = this;
        }
        private async void OnPickFolderClicked(object sender, EventArgs e)
        {
            var pickedFolder = await _folderPicker.PickFolder();

            FolderLabel.Text = pickedFolder;

            while (Items.Count() > 0)
            {
                Items.RemoveAt(0);
            }

            var files = Directory.EnumerateFiles(pickedFolder);
            foreach (var file in files)
            {
                Items.Add(
                    new Item { Name = file }
                );
            }
        }
    }
}
