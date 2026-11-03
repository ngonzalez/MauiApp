using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1
{
    public partial class UserAvatar : ContentView
    {
        public static readonly BindableProperty SizeProperty =
            BindableProperty.Create(nameof(Size), typeof(double), typeof(UserAvatar), 32.0,
                propertyChanged: (bindable, oldValue, newValue) => ((UserAvatar)bindable).Resize((double)newValue));

        // The circle's diameter
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public UserAvatar()
        {
            InitializeComponent();
            BindingContext = CurrentUserAvatar.Shared;
            Resize(Size);
        }

        private void Resize(double size)
        {
            circle.WidthRequest = size;
            circle.HeightRequest = size;
            circle.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(size / 2) };
            initials.FontSize = Math.Round(size * 0.4);
            icon.FontSize = Math.Round(size * 0.45);
        }
    }
}
