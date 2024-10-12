using Android.Content;
using Android.Runtime;
using Android.Views;
using Graphics.Core.Helpers;
using Java.Interop;
using Microsoft.Maui.Animations;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Tests.AndroidApp.Controls;
using Tests.AndroidApp.Platforms.Android.Helpers;
using Format = Android.Graphics.Format;

namespace Tests.AndroidApp.Platforms.Android.Controls;

internal sealed unsafe class VkSurface(ANativeWindow* window) : IVkSurface, IDisposable
{
    private readonly Vk _vk = Vk.GetApi();
    private readonly Alloter _alloter = new();

    ~VkSurface()
    {
        Dispose();
    }

    public VkNonDispatchableHandle Create<T>(VkHandle instance, T* allocator) where T : unmanaged
    {
        if (!_vk.TryGetInstanceExtension(new Instance(instance.Handle), out KhrAndroidSurface androidSurface))
        {
            throw new InvalidOperationException("KHR Android Surface extension is not supported.");
        }

        AndroidSurfaceCreateInfoKHR createInfo = new()
        {
            SType = StructureType.AndroidSurfaceCreateInfoKhr,
            Window = (nint*)window
        };

        SurfaceKHR surface;
        if (androidSurface.CreateAndroidSurface(new Instance(instance.Handle), &createInfo, (AllocationCallbacks*)allocator, &surface) != Result.Success)
        {
            throw new InvalidOperationException("Failed to create Android surface.");
        }

        return new VkNonDispatchableHandle(surface.Handle);
    }

    public byte** GetRequiredExtensions(out uint count)
    {
        count = 2;

        return _alloter.Allocate([KhrSurface.ExtensionName, KhrAndroidSurface.ExtensionName]);
    }

    public void Dispose()
    {
        _alloter.Dispose();
        _vk.Dispose();

        GC.SuppressFinalize(this);
    }
}

internal sealed unsafe class SwapChainPanel : SurfaceView, ISurfaceHolderCallback
{
    private readonly ISwapChainPanel _swapChainPanel;

    private ANativeWindow* _window;
    private Ticker? _ticker;

    public SwapChainPanel(Context context, ISwapChainPanel swapChainPanel) : base(context)
    {
        _swapChainPanel = swapChainPanel;

        Holder?.AddCallback(this);
    }

    public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
    {
        if (_window == null)
        {
            CreateSurface();
        }
        else
        {
            _swapChainPanel.Resize((uint)width, (uint)height);
        }
    }

    public void SurfaceCreated(ISurfaceHolder holder)
    {
        CreateSurface();
    }

    public void SurfaceDestroyed(ISurfaceHolder holder)
    {
        DestroySurface();
    }

    private void CreateSurface()
    {
        DestroySurface();

        _window = NativeActivity.ANativeWindowFromSurface(JniEnvironment.EnvironmentPointer, Holder!.Surface!.Handle);

        _swapChainPanel.CreateSwapChainPanel(new VkSurface(_window));

        _ticker = new Ticker();
        _ticker.Fire += () =>
        {
            lock (_swapChainPanel)
            {
                _swapChainPanel.Update();
                _swapChainPanel.Render();
            }
        };
        _ticker.Start();
    }

    private void DestroySurface()
    {
        if (_window != null)
        {
            _swapChainPanel.DestroySwapChainPanel();

            NativeActivity.ANativeWindowRelease(_window);

            _window = null;
        }

        if (_ticker != null)
        {
            _ticker.Stop();

            _ticker = null;
        }
    }
}
