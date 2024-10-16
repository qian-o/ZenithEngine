using Tests.AndroidApp.ViewModels;

namespace Tests.AndroidApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new MainViewModel();
    }
}

