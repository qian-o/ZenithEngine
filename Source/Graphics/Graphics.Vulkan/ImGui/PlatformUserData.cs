using System.Runtime.InteropServices;
using Graphics.Core;

namespace Graphics.Vulkan;

internal unsafe struct PlatformUserData
{
    public GCHandle ImGuiWindow;

    public ImGuiWindow GetImGuiWindow() => (ImGuiWindow)ImGuiWindow.Target!;

    public Window GetWindow() => GetImGuiWindow().Window;

    public static PlatformUserData* Alloc(ImGuiWindow imGuiWindow)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)Marshal.AllocHGlobal(sizeof(PlatformUserData));
        platformUserData->ImGuiWindow = GCHandle.Alloc(imGuiWindow);

        return platformUserData;
    }

    public static void Free(PlatformUserData* platformUserData)
    {
        if (platformUserData->ImGuiWindow.IsAllocated)
        {
            platformUserData->ImGuiWindow.Free();
            Marshal.FreeHGlobal((IntPtr)platformUserData);
        }
    }
}
