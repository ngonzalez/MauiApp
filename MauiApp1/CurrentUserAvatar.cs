using System.ComponentModel;

namespace MauiApp1
{
    // The signed-in user's picture, for the flyout header and the edit
    // account page (UserAvatar): Authenticate.setCurrentUser updates it, so
    // every avatar follows a new picture, a removed one or a sign out
    public class CurrentUserAvatar : INotifyPropertyChanged
    {
        public static readonly CurrentUserAvatar Shared = new CurrentUserAvatar();

        private User? _user;

        private ImageSource? _picture;

        public event PropertyChangedEventHandler? PropertyChanged;

        private CurrentUserAvatar()
        {
        }

        public bool IsSignedIn => _user?.id != null;

        // The picture from avatarUrl: its URL changes with the picture, so
        // the image cache never shows an old one
        public ImageSource? Picture => _picture;

        public bool HasPicture => _picture != null;

        public bool HasNoPicture => _picture == null;

        public string Initials => AvatarRules.Initials(_user?.firstName, _user?.lastName, _user?.emailAddress);

        // A person icon when there are no initials either
        public bool HasNoInitials => HasNoPicture && Initials.Length == 0;

        public bool HasInitials => HasNoPicture && Initials.Length > 0;

        public string Name
        {
            get
            {
                string name = $"{_user?.firstName} {_user?.lastName}".Trim();
                return name.Length > 0 ? name : _user?.emailAddress ?? "";
            }
        }

        public string AccountName => _user?.accountName ?? "";

        public void Update(User? user)
        {
            string? oldUrl = _user?.avatarUrl;
            _user = user;
            if (user?.avatarUrl != oldUrl || (_picture == null) != string.IsNullOrEmpty(user?.avatarUrl))
            {
                _picture = Uri.TryCreate(user?.avatarUrl, UriKind.Absolute, out Uri? uri) ? ImageSource.FromUri(uri) : null;
            }

            if (MainThread.IsMainThread)
            {
                Changed();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(Changed);
            }
        }

        private void Changed()
        {
            foreach (var name in new[] { nameof(IsSignedIn), nameof(Picture), nameof(HasPicture), nameof(HasNoPicture), nameof(Initials), nameof(HasNoInitials), nameof(HasInitials), nameof(Name), nameof(AccountName) })
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
