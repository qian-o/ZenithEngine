using Android.Views;
using Microsoft.Maui.Handlers;
using Tests.AndroidApp.Controls;
using Tests.AndroidApp.Platforms.Android.Controls;

namespace Tests.AndroidApp.Handlers;

internal sealed class SwapChainPanelHandler : ViewHandler<SwapChainPanel, SurfaceView>
{
    public static PropertyMapper<SwapChainPanel, SwapChainPanelHandler> mapper = new(ViewMapper)
    {
    };

    public static CommandMapper<SwapChainPanel, SwapChainPanelHandler> commandMapper = new(ViewCommandMapper)
    {
    };

    public SwapChainPanelHandler() : base(mapper, commandMapper)
    {
    }

    protected override SurfaceView CreatePlatformView()
    {
        return new VkSurfaceView(Context, VirtualView);
    }
}
