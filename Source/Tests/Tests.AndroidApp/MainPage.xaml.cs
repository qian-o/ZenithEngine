using Graphics.Vulkan;

namespace Tests.AndroidApp;

public partial class MainPage : ContentPage
{
    private int count;

    public MainPage()
    {
        InitializeComponent();

        using Context context = new();

        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice());
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        CounterBtn.Text = count == 1 ? $"Clicked {count} time" : $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}

