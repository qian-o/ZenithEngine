using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Renderer.Components.Scenes;

namespace Renderer.Components;

internal sealed class MainScene : DisposableObject
{
    private readonly Window _window;
    private readonly Context _context;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ImGuiController _imGuiController;
    private readonly CommandList _commandList;

    private bool _firstFrame = true;

    public MainScene(Window window)
    {
        if (!window.IsInitialized)
        {
            throw new InvalidOperationException("Window is not initialized.");
        }

        _window = window;
        _context = new Context();
        _graphicsDevice = _context.CreateGraphicsDevice(_context.EnumeratePhysicalDevices().First(), window);
        _imGuiController = new ImGuiController(_graphicsDevice, _window.IWindow, _window.InputContext);
        _commandList = _graphicsDevice.ResourceFactory.CreateGraphicsCommandList();

        _window.Update += Window_Update;
        _window.Render += Window_Render;
        _window.Resize += Window_Resize;

        SubScenes.Add(CreateSubScene<TestScene>());
    }

    public List<SubScene> SubScenes { get; } = [];

    public TSubScene CreateSubScene<TSubScene>() where TSubScene : SubScene
    {
        return (TSubScene)Activator.CreateInstance(typeof(TSubScene), _graphicsDevice, _imGuiController)!;
    }

    protected override void Destroy()
    {
        foreach (SubScene subScene in SubScenes)
        {
            subScene.Dispose();
        }
        _commandList.Dispose();
        _imGuiController.Dispose();
        _graphicsDevice.Dispose();
        _context.Dispose();
    }

    private void Window_Update(object? sender, UpdateEventArgs e)
    {
        foreach (SubScene subScene in SubScenes)
        {
            subScene.Update(e);
        }
    }

    private void Window_Render(object? sender, RenderEventArgs e)
    {
        _imGuiController.Update(e.DeltaTime);

        if (_firstFrame)
        {
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            _firstFrame = false;
        }

        ImGui.DockSpaceOverViewport();

        foreach (SubScene subScene in SubScenes)
        {
            subScene.Render(e);
        }

        _commandList.Begin();
        _commandList.SetFramebuffer(_graphicsDevice.Swapchain.Framebuffer);
        {
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1.0f);

            _imGuiController.Render(_commandList);
        }
        _commandList.End();

        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
    }

    private void Window_Resize(object? sender, ResizeEventArgs e)
    {
        _graphicsDevice.Resize(e.Width, e.Height);
    }
}
