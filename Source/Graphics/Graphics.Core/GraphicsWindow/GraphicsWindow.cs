using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Graphics.Core;

public unsafe partial class GraphicsWindow : DisposableObject
{
    private readonly IWindow _window;
    private readonly IInputContext _inputContext;
    private readonly IMouse _mouse;
    private readonly IKeyboard _keyboard;

    internal GraphicsWindow(IWindow window, IInputContext inputContext)
    {
        _window = window;
        _inputContext = inputContext;
        _mouse = _inputContext.Mice[0];
        _keyboard = _inputContext.Keyboards[0];

        Assembly();
    }

    public nint Handle => _window.Handle;

    public IVkSurface? VkSurface => _window.VkSurface;

    protected override void Destroy()
    {
        _window.Close();

        _inputContext.Dispose();
        _window.Dispose();
    }

    private void Assembly()
    {
        AssemblyStatusEvent();
        AssemblyMouseEvent();
        AssemblyKeyboardEvent();
    }

    public static GraphicsWindow CreateWindowByVulkan()
    {
        WindowOptions windowOptions = WindowOptions.DefaultVulkan;
        windowOptions.IsVisible = false;
        windowOptions.API = new GraphicsAPI()
        {
            API = ContextAPI.Vulkan,
            Profile = ContextProfile.Core,
            Flags = ContextFlags.ForwardCompatible,
            Version = new APIVersion(1, 3)
        };

        IWindow window = Window.Create(windowOptions);
        window.Initialize();

        return new GraphicsWindow(window, window.CreateInput());
    }
}