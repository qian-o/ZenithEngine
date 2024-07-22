using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Renderer.Components;
using Renderer.Controls;
using Renderer.Scenes;

namespace Renderer;

internal sealed unsafe class MainWindow : DisposableObject
{
    private readonly Window _window;
    private readonly Context _context;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ImGuiController _imGuiController;
    private readonly CommandList _commandList;
    private readonly List<Control> _controls;
    private readonly List<Scene> _scenes;

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
        _imGuiController = new ImGuiController(_graphicsDevice,
                                               _window.IWindow,
                                               _window.InputContext,
                                               new ImGuiFontConfig("Assets/Fonts/MSYH.TTC", 14, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()));
        _commandList = _graphicsDevice.ResourceFactory.CreateGraphicsCommandList();
        _controls = [];
        _scenes = [];

        _window.Update += Window_Update;
        _window.Render += Window_Render;
        _window.Resize += Window_Resize;

        Initialize();
    }

    public Context Context => _context;

    public GraphicsDevice GraphicsDevice => _graphicsDevice;

    public ResourceFactory ResourceFactory => _graphicsDevice.ResourceFactory;

    public ImGuiController ImGuiController => _imGuiController;

    protected override void Destroy()
    {
        foreach (Scene scene in _scenes)
        {
            scene.Dispose();
        }
        foreach (Control control in _controls)
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

        _controls.Add(menuBar);

        TestScene testScene = new(this);

        _scenes.Add(testScene);
    }

    private void Window_Update(object? sender, UpdateEventArgs e)
    {
        if (App.Settings.IsMultiThreadedRendering)
        {
            Parallel.ForEach(_controls, (control) =>
            {
                control.Update(e);
            });

            Parallel.ForEach(_scenes, (scene) =>
            {
                scene.Update(e);
            });
        }
        else
        {
            foreach (Control control in _controls)
            {
                control.Update(e);
            }

            foreach (Scene scene in _scenes)
            {
                scene.Update(e);
            }
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

        foreach (Control control in _controls)
        {
            control.Render(e);
        }

        foreach (Scene scene in _scenes)
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
