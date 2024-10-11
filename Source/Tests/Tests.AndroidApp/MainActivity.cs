
using Graphics.Vulkan;

namespace Tests.AndroidApp;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        using Context context = new();

        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice());
    }
}