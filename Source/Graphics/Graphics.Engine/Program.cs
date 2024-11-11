using Graphics.Engine;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Windowing;
using Graphics.Windowing.Events;
using Silk.NET.Maths;

// 1. Use Dynamic Rendering instead of RenderPass.
// 2. Reduce unnecessary assignment operations.
// 3. Use slang instead of HLSL.

using Context context = Context.Create(Backend.Vulkan);

context.CreateDevice(true);

SwapChain swapChain = null!;

SdlWindow window = new();
window.Loaded += Window_Loaded;
window.Update += Window_Update;
window.Render += Window_Render;
window.SizeChanged += Window_SizeChanged;
window.Unloaded += Window_Unloaded;

window.Show();

WindowManager.Loop();

void Window_Loaded(object? sender, EventArgs e)
{
    SwapChainDesc swapChainDesc = SwapChainDesc.Default(window.Surface);
    swapChain = context.Factory.CreateSwapChain(in swapChainDesc);

    Console.WriteLine("Initialization completed.");
}

void Window_Update(object? sender, TimeEventArgs e)
{
}

void Window_Render(object? sender, TimeEventArgs e)
{
}

void Window_SizeChanged(object? sender, ValueEventArgs<Vector2D<int>> e)
{
    swapChain.Resize();
}

void Window_Unloaded(object? sender, EventArgs e)
{
    swapChain.Dispose();
}