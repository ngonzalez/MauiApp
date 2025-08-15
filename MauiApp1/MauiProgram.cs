using Microsoft.Extensions.Logging;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
//#if WINDOWS
            builder.Services.AddTransient<IFolderPicker, MauiApp1.Platforms.Windows.FolderPicker>();
            builder.Services.AddTransient<AccountPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<App>();
//#endif

            builder.Logging.AddDebug();

            return builder.Build();

        }
    }
}
