using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Renderer.Components;
using Renderer.Scenes;

namespace Renderer;

internal sealed class MainWindow : DisposableObject
{
    private readonly Window _window;
    private readonly Context _context;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ImGuiController _imGuiController;
    private readonly CommandList _commandList;

    private bool _firstFrame = true;

    public MainWindow(Window window)
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

        Scenes.Add(CreateScene<TestScene>());
    }

    public List<Scene> Scenes { get; } = [];

    public TScene CreateScene<TScene>() where TScene : Scene
    {
        return (TScene)Activator.CreateInstance(typeof(TScene), _graphicsDevice, _imGuiController)!;
    }

    protected override void Destroy()
    {
        foreach (Scene subScene in Scenes)
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
        foreach (Scene scene in Scenes)
        {
            scene.Update(e);
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

        foreach (Scene scene in Scenes)
        {
            scene.Render(e);
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
