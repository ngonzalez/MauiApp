using MauiApp1.Platforms.Windows;
using MauiApp1.WinUI;
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
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<AppShellViewModel>();
            builder.Services.AddTransient<IFolderPicker, MauiApp1.Platforms.Windows.FolderPicker>();
            builder.Services.AddTransient<IAuthenticate, MauiApp1.Platforms.Windows.Authenticate>();
            builder.Services.AddTransient<IApiService, MauiApp1.Platforms.Windows.ApiService>();
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<App>();
            //#endif

            builder.Logging.AddDebug();

            return builder.Build();

        }
    }
}
