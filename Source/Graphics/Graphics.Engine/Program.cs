using Graphics.Engine;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Windowing;
using Graphics.Windowing.Events;
using Silk.NET.Maths;

// 1. Use Dynamic Rendering instead of RenderPass.
// 2. Reduce unnecessary assignment operations.
// 3. Use slang instead of HLSL.
// 4. Silk.NET 3.0 use VkStruct == NULL.

using Context context = Context.Create(Backend.Vulkan);

context.CreateDevice(true);

CommandProcessor commandProcessor = context.Factory.CreateCommandProcessor();

SwapChain swapChain = null!;

SdlWindow window = new();
window.Loaded += Loaded;
window.Update += Update;
window.Render += Render;
window.SizeChanged += SizeChanged;
window.Unloaded += Unloaded;

window.Show();

WindowManager.Loop();

void Loaded(object? sender, EventArgs e)
{
    SwapChainDesc swapChainDesc = SwapChainDesc.Default(window.Surface);
    swapChain = context.Factory.CreateSwapChain(in swapChainDesc);

    Console.WriteLine("Initialization completed.");
}

void Update(object? sender, TimeEventArgs e)
{
}

void Render(object? sender, TimeEventArgs e)
{
    CommandBuffer commandBuffer = commandProcessor.CommandBuffer();

    commandBuffer.Begin();
    commandBuffer.BeginRendering(swapChain.FrameBuffer, ClearValue.Default);

    // render code...

    commandBuffer.EndRendering();
    commandBuffer.End();
    commandBuffer.Commit();

    commandProcessor.Submit();
    commandProcessor.WaitIdle();

    swapChain.Present();
}

void SizeChanged(object? sender, ValueEventArgs<Vector2D<int>> e)
{
    swapChain.Resize();
}

void Unloaded(object? sender, EventArgs e)
{
    swapChain.Dispose();
}