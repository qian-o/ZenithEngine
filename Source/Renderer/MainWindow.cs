using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Components;
using Renderer.Controls;
using Renderer.Scenes;

namespace Renderer;

internal sealed unsafe class MainWindow : DisposableObject
{
    private readonly GWindow _gWindow;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ImGuiController _imGuiController;
    private readonly CommandList _commandList;
    private readonly List<Control> _controls;
    private readonly List<Scene> _scenes;

    public MainWindow(GWindow gWindow)
    {
        _gWindow = gWindow;
        _graphicsDevice = App.Context.CreateGraphicsDevice(App.Context.EnumeratePhysicalDevices().First(), _gWindow);
        _imGuiController = new ImGuiController(_gWindow,
                                               _graphicsDevice,
                                               new ImGuiFontConfig("Assets/Fonts/MSYH.TTC", 14, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()));
        _commandList = _graphicsDevice.ResourceFactory.CreateGraphicsCommandList();
        _controls = [];
        _scenes = [];

        _gWindow.Update += Window_Update;
        _gWindow.Render += Window_Render;
        _gWindow.Resize += Window_Resize;

        Initialize();
    }

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
    }

    private void Initialize()
    {
        MenuBar menuBar = new(this);

        _controls.Add(menuBar);

        for (int i = 0; i < 4; i++)
        {
            _scenes.Add(new TestScene(this) { Title = $"Scene {i + 1}" });
        }
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

        foreach (Control control in _controls)
        {
            control.Render(e);
        }

        foreach (Scene scene in _scenes)
        {
            scene.Render(e);
        }

        _commandList.Begin();
        _commandList.SetFramebuffer(_graphicsDevice.MainSwapchain.Framebuffer);
        {
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1.0f);

            _imGuiController.Render(_commandList);
        }
        _commandList.End();

        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();

        _imGuiController.PlatformSwapBuffers();
    }

    private void Window_Resize(object? sender, ResizeEventArgs e)
    {
        _graphicsDevice.MainSwapchain.Resize(e.Width, e.Height);
    }
}
