using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Java.Interop;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Tests.AndroidApp.Controls;
using Tests.AndroidApp.Platforms.Android.Helpers;

namespace Tests.AndroidApp.Platforms.Android.Controls;

internal sealed unsafe class VkSurface(ANativeWindow* window) : IVkSurface
{
    public VkNonDispatchableHandle Create<T>(VkHandle instance, T* allocator) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public byte** GetRequiredExtensions(out uint count)
    {
        throw new NotImplementedException();
    }
}

internal sealed unsafe class VkSurfaceView : SurfaceView, ISurfaceHolderCallback
{
    private readonly ISwapChainPanel _swapChainPanel;

    private ANativeWindow* _window;

    public VkSurfaceView(Context context, ISwapChainPanel swapChainPanel) : base(context)
    {
        _swapChainPanel = swapChainPanel;

        Holder?.AddCallback(this);
    }

    public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
    {
        CreateSurface();
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
    }

    private void DestroySurface()
    {
        if (_window != null)
        {
            _swapChainPanel.DestroySwapChainPanel();

            NativeActivity.ANativeWindowRelease(_window);

            _window = null;
        }
    }
}
