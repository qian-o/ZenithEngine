using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Components;
using Renderer.Controls;
using Renderer.Models;
using Renderer.Scenes;

namespace Renderer;

internal unsafe static class App
{
    private static readonly Window _window;
    private static readonly Context _context;
    private static readonly GraphicsDevice _graphicsDevice;
    private static readonly ImGuiController _imGuiController;
    private static readonly CommandList _commandList;
    private static readonly List<Control> _controls;
    private static readonly List<Scene> _scenes;
    private static readonly Settings _settings;

    static App()
    {
        Window window = Window.CreateWindowByVulkan();
        Context context = new();
        GraphicsDevice graphicsDevice = context.CreateGraphicsDevice(context.EnumeratePhysicalDevices().First(), window);
        ImGuiController imGuiController = new(window, graphicsDevice, new ImGuiFontConfig("Assets/Fonts/MSYH.TTC", 14, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()));
        CommandList commandList = graphicsDevice.ResourceFactory.CreateGraphicsCommandList();

        _window = window;
        _context = context;
        _graphicsDevice = graphicsDevice;
        _imGuiController = imGuiController;
        _commandList = commandList;
        _controls = [];
        _scenes = [];
        _settings = new();
    }

    public static GraphicsDevice GraphicsDevice => _graphicsDevice;

    public static ResourceFactory ResourceFactory => _graphicsDevice.ResourceFactory;

    public static ImGuiController ImGuiController => _imGuiController;

    public static Settings Settings => _settings;

    public static void Run()
    {
        _window.MinimumSize = new(100, 100);
        _window.Load += Window_Load;
        _window.Update += Window_Update;
        _window.Render += Window_Render;
        _window.Resize += Window_Resize;
        _window.Closing += Window_Closing;

        _window.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        _controls.Add(new MenuBar());

        _scenes.Add(new TestScene());
        _scenes.Add(new GLTFScene());
    }

    private static void Window_Update(object? sender, UpdateEventArgs e)
    {
        _imGuiController.Update(e.DeltaTime);

        if (_settings.IsMultiThreadedRendering)
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

    private static void Window_Render(object? sender, RenderEventArgs e)
    {
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

    private static void Window_Resize(object? sender, ResizeEventArgs e)
    {
        _graphicsDevice.MainSwapchain.Resize(e.Width, e.Height);
    }

    private static void Window_Closing(object? sender, ClosingEventArgs e)
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
}
