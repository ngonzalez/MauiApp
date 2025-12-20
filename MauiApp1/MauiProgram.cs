using CommunityToolkit.Maui;
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
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FontAwesome7FreeSolid");
                    fonts.AddFont("Font Awesome 7 Free-Regular-400.otf", "FontAwesome7FreeRegular");
                    fonts.AddFont("Font Awesome 7 Brands-Regular-400.otf", "FontAwesome7BrandsRegular");
                });

            //#if WINDOWS
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<AppShellViewModel>();
            builder.Services.AddTransient<IFolderPicker, MauiApp1.Platforms.Windows.FolderPicker>();
            builder.Services.AddTransient<IAuthenticate, MauiApp1.Platforms.Windows.Authenticate>();
            builder.Services.AddTransient<IApiService, MauiApp1.Platforms.Windows.ApiService>();
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AccountPage>();
            builder.Services.AddTransient<MainAccountPage>();
            builder.Services.AddTransient<DisplayPage>();
            builder.Services.AddTransient<ShowImageFilePage>();
            builder.Services.AddTransient<App>();
            //#endif

            builder.Logging.AddDebug();

            return builder.Build();

        }
    }
}
