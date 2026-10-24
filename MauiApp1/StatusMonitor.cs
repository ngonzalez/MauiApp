using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MauiApp1.Resources.Strings;

namespace MauiApp1
{
    // A system event of the status page (status.appshare.site), from the
    // backend's GET /system_events
    public class SystemStatusEvent
    {
        public int id { get; set; }
        // incident, maintenance, deploy or info
        public string eventType { get; set; } = "";
        // website, api, streaming or registration; none for every service
        public string? service { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        // ISO 8601
        public string occurredAt { get; set; } = "";
        public string? endedAt { get; set; }
        public bool ongoing { get; set; }

        // What identifies the event as the user saw it: hidden, it comes back
        // once it changes (a new title, an end)
        public string DismissKey => $"{id}|{eventType}|{title}|{endedAt}";
    }

    // Watches the status page's ongoing events for the banner at the top of
    // every page (StatusBanner): on launch, then every five minutes; remembers
    // the ones the user hid
    public class StatusMonitor : INotifyPropertyChanged
    {
        public static readonly StatusMonitor Shared = new StatusMonitor();

        public const string StatusUrl = "https://api.appshare.site/system_events";
        public const string PageUrl = "https://status.appshare.site";
        private const string DismissedKey = "dismissedStatusEvents";
        // The kinds the banner shows, the most serious first: a deploy isn't
        // worth telling the user
        private static readonly string[] ShownTypes = { "incident", "maintenance", "info" };

        private readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        private List<SystemStatusEvent> _events = new List<SystemStatusEvent>();
        private readonly HashSet<string> _dismissed;
        private bool _started;

        public event PropertyChangedEventHandler? PropertyChanged;

        private StatusMonitor()
        {
            _dismissed = new HashSet<string>(Preferences.Get(DismissedKey, "").Split('\n', StringSplitOptions.RemoveEmptyEntries));
        }

        // The ongoing events the user hasn't hidden, the most serious first
        public List<SystemStatusEvent> Shown => _events
            .Where(e => e.ongoing && ShownTypes.Contains(e.eventType) && !_dismissed.Contains(e.DismissKey))
            .OrderBy(e => Array.IndexOf(ShownTypes, e.eventType))
            .ToList();

        public SystemStatusEvent? Current => Shown.FirstOrDefault();

        public bool IsVisible => Current != null;

        public string Title => Current?.title ?? "";

        // When the event began: its time today, or its date and time
        public string Since
        {
            get
            {
                if (Current == null || !DateTimeOffset.TryParse(Current.occurredAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) return "";
                var local = date.ToLocalTime();
                var time = local.Date == DateTime.Today ? local.ToString("t", CultureInfo.CurrentCulture) : local.ToString("g", CultureInfo.CurrentCulture);
                return AppResources.Format("StatusSince", time);
            }
        }

        public string More => Shown.Count > 1 ? AppResources.Count("StatusMore", Shown.Count - 1) : "";

        public bool HasMore => Shown.Count > 1;

        // Red for an incident, amber for a maintenance, blue for a note
        public Color BackgroundColor => Current?.eventType switch
        {
            "incident" => Color.FromArgb("#E84C3D"),
            "maintenance" => Color.FromArgb("#F7BA33"),
            _ => Color.FromArgb("#3385DB"),
        };

        public Color TextColor => Current?.eventType == "maintenance" ? Color.FromArgb("#D9000000") : Colors.White;

        public string Icon => Current?.eventType switch
        {
            "incident" => "⚠",
            "maintenance" => "",
            _ => "",
        };

        // Check now, then every five minutes, once for the whole app
        public void Start()
        {
            if (_started) return;
            _started = true;
            _ = Task.Run(async () =>
            {
                using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
                do { await Refresh(); } while (await timer.WaitForNextTickAsync());
            });
        }

        // The status now; a failure keeps the last one (the banner isn't worth an error)
        public async Task Refresh()
        {
            try
            {
                var json = await _httpClient.GetStringAsync(StatusUrl);
                using var document = JsonDocument.Parse(json);
                var events = document.RootElement.GetProperty("events").Deserialize<List<SystemStatusEvent>>() ?? new List<SystemStatusEvent>();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _events = events;
                    Changed();
                });
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"StatusMonitor: {exception.Message}");
            }
        }

        // Hide the event until it changes
        public void Dismiss()
        {
            if (Current == null) return;
            _dismissed.Add(Current.DismissKey);
            // only the events still listed: the set doesn't grow forever
            var listed = _events.Select(e => e.DismissKey).ToHashSet();
            Preferences.Set(DismissedKey, string.Join('\n', _dismissed.Where(listed.Contains)));
            Changed();
        }

        private void Changed()
        {
            foreach (var name in new[] { nameof(IsVisible), nameof(Title), nameof(Since), nameof(More), nameof(HasMore), nameof(BackgroundColor), nameof(TextColor), nameof(Icon) })
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
