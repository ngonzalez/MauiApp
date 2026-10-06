using System.Globalization;
using System.Resources;

namespace MauiApp1.Resources.Strings
{
    // The interface texts, from AppResources.resx (English) and its French,
    // Spanish and German versions, in the language of Windows
    public static class AppResources
    {
        private static readonly ResourceManager Manager =
            new ResourceManager("MauiApp1.Resources.Strings.AppResources", typeof(AppResources).Assembly);

        // The text of a key; the key itself if it is missing, so it shows up
        public static string Get(string key)
        {
            return Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, Get(key), args);
        }

        // "<key>One" for 1, "<key>Many" otherwise, with the count as {0}
        public static string Count(string key, int count)
        {
            return Format(key + (count == 1 ? "One" : "Many"), count);
        }
    }

    // In XAML: Text="{strings:Translate SignIn}"
    [ContentProperty(nameof(Key))]
    [AcceptEmptyServiceProvider]
    public class TranslateExtension : IMarkupExtension<string>
    {
        public string Key { get; set; } = "";

        public string ProvideValue(IServiceProvider serviceProvider)
        {
            return AppResources.Get(Key);
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}
