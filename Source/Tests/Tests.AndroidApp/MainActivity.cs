using Graphics.Vulkan;
using Silk.NET.Windowing.Sdl.Android;

namespace Tests.AndroidApp;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : SilkActivity
{
    protected override void OnRun()
    {
        using Context context = new();

        foreach (PhysicalDevice physicalDevice in context.EnumeratePhysicalDevices())
        {
            Console.WriteLine(physicalDevice.Name);
        }
    }
}