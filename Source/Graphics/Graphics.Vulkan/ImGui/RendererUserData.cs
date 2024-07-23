using System.Runtime.InteropServices;

namespace Graphics.Vulkan;

internal unsafe struct RendererUserData
{
    public GCHandle GraphicsDevice;

    public GraphicsDevice GetGraphicsDevice() => (GraphicsDevice)GraphicsDevice.Target!;

    public static RendererUserData* Alloc(GraphicsDevice graphicsDevice)
    {
        RendererUserData* rendererData = (RendererUserData*)Marshal.AllocHGlobal(sizeof(RendererUserData));
        rendererData->GraphicsDevice = GCHandle.Alloc(graphicsDevice);

        return rendererData;
    }

    public static void Free(RendererUserData* rendererData)
    {
        if (rendererData->GraphicsDevice.IsAllocated)
        {
            rendererData->GraphicsDevice.Free();
            Marshal.FreeHGlobal((IntPtr)rendererData);
        }
    }
}
