using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Renderer.Components;
using Renderer.Controls;
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

        Initialize();
    }

    public Context Context => _context;

    public GraphicsDevice GraphicsDevice => _graphicsDevice;

    public ImGuiController ImGuiController => _imGuiController;

    public List<Control> Controls { get; } = [];

    public List<Scene> Scenes { get; } = [];

    protected override void Destroy()
    {
        foreach (Scene scene in Scenes)
        {
            scene.Dispose();
        }
        foreach (Control control in Controls)
        {
            control.Dispose();
        }
        _commandList.Dispose();
        _imGuiController.Dispose();
        _graphicsDevice.Dispose();
        _context.Dispose();
    }

    private void Initialize()
    {
        MenuBar menuBar = new(this);

        Controls.Add(menuBar);

        TestScene testScene = new(this);

        Scenes.Add(testScene);
    }

    private void Window_Update(object? sender, UpdateEventArgs e)
    {
        foreach (Control control in Controls)
        {
            control.Update(e);
        }

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

        foreach (Control control in Controls)
        {
            control.Render(e);
        }

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
