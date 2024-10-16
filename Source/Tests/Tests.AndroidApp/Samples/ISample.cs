using Graphics.Vulkan;

namespace Tests.AndroidApp.Samples;

internal interface ISample
{
    void Load(Swapchain swapchain);

    void Update(Swapchain swapchain, float width, float height, float deltaTime, float totalTime);

    void Render(Swapchain swapchain, float deltaTime, float totalTime);

    void Unload();
}
