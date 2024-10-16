using CommunityToolkit.Maui.Behaviors;
using Tests.AndroidApp.Views;

namespace Tests.AndroidApp.Controls;

public abstract class ShellPage : ContentPage
{
    public ShellPage()
    {
        ControlTemplate = new ControlTemplate(() =>
        {
            ImageButton imageButton = new()
            {
                Source = "menu.svg",
                WidthRequest = 30,
                HeightRequest = 30,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(15),
                CornerRadius = 4,
                Padding = new Thickness(2, 0)
            };

            IconTintColorBehavior iconTintColorBehavior = new();
            iconTintColorBehavior.SetAppThemeColor(IconTintColorBehavior.TintColorProperty,
                                                  (Color)Application.Current!.Resources["Black"],
                                                  (Color)Application.Current!.Resources["White"]);

            imageButton.Behaviors.Add(iconTintColorBehavior);

            imageButton.SetAppThemeColor(BackgroundColorProperty,
                                         (Color)Application.Current!.Resources["Gray200"],
                                         (Color)Application.Current!.Resources["Gray600"]);

            imageButton.Clicked += ShellPage_Clicked;

            Grid grid =
            [
                new ContentPresenter(),
                imageButton
            ];

            return grid;
        });
    }

    private void ShellPage_Clicked(object? sender, EventArgs e)
    {
        if (Application.Current!.MainPage is AppShell appShell)
        {
            appShell.FlyoutIsPresented = true;
        }
    }
}