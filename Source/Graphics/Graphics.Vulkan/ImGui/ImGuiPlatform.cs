using System.Numerics;
using System.Runtime.InteropServices;
using Graphics.Core;
using Hexa.NET.ImGui;

namespace Graphics.Vulkan;

internal static unsafe class ImGuiPlatform
{
    private struct PlatformUserData
    {
        public GCHandle Window;

        public Window GetWindow() => (Window)Window.Target!;

        public static PlatformUserData* Alloc(Window window)
        {
            PlatformUserData* viewportData = (PlatformUserData*)Marshal.AllocHGlobal(sizeof(PlatformUserData));
            viewportData->Window = GCHandle.Alloc(window);

            return viewportData;
        }

        public static void Free(PlatformUserData* platformUserData)
        {
            if (platformUserData->Window.IsAllocated)
            {
                platformUserData->Window.Free();
                Marshal.FreeHGlobal((IntPtr)platformUserData);
            }
        }
    }

    private struct RendererUserData
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

    public static void Initialize(GraphicsDevice graphicsDevice, Window window)
    {
        ImGuiViewport* mainViewport = ImGui.GetMainViewport();
        mainViewport->PlatformHandle = (void*)window.Handle;
        mainViewport->PlatformUserData = PlatformUserData.Alloc(window);
        mainViewport->RendererUserData = RendererUserData.Alloc(graphicsDevice);

        ImGuiPlatformIOPtr imGuiPlatformIOPtr = ImGui.GetPlatformIO();
        imGuiPlatformIOPtr.PlatformCreateWindow = (void*)Marshal.GetFunctionPointerForDelegate<PlatformCreateWindow>(CreateWindow);
        imGuiPlatformIOPtr.PlatformDestroyWindow = (void*)Marshal.GetFunctionPointerForDelegate<PlatformDestroyWindow>(DestroyWindow);
        imGuiPlatformIOPtr.PlatformShowWindow = (void*)Marshal.GetFunctionPointerForDelegate<PlatformShowWindow>(ShowWindow);
        imGuiPlatformIOPtr.PlatformSetWindowPos = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowPos>(SetWindowPos);
        imGuiPlatformIOPtr.PlatformGetWindowPos = (void*)Marshal.GetFunctionPointerForDelegate<PlatformGetWindowPos>(GetWindowPos);
        imGuiPlatformIOPtr.PlatformSetWindowSize = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowSize>(SetWindowSize);
        imGuiPlatformIOPtr.PlatformGetWindowSize = (void*)Marshal.GetFunctionPointerForDelegate<PlatformGetWindowSize>(GetWindowSize);
        imGuiPlatformIOPtr.PlatformSetWindowFocus = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowFocus>(SetWindowFocus);
        imGuiPlatformIOPtr.PlatformGetWindowFocus = (void*)Marshal.GetFunctionPointerForDelegate<PlatformGetWindowFocus>(GetWindowFocus);
        imGuiPlatformIOPtr.PlatformGetWindowMinimized = (void*)Marshal.GetFunctionPointerForDelegate<PlatformGetWindowMinimized>(GetWindowMinimized);
        imGuiPlatformIOPtr.PlatformSetWindowTitle = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowTitle>(SetWindowTitle);
    }

    private static void CreateWindow(ImGuiViewport* viewport)
    {
        Window window = Window.CreateWindowByVulkan();

        viewport->PlatformUserData = PlatformUserData.Alloc(window);
        viewport->RendererUserData = ImGui.GetMainViewport().RendererUserData;
    }

    private static void DestroyWindow(ImGuiViewport* viewport)
    {
        if (viewport->PlatformUserData != null)
        {
            PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

            Window window = platformUserData->GetWindow();

            window.Exit();

            PlatformUserData.Free(platformUserData);

            viewport->PlatformUserData = null;
        }

        if (viewport->PlatformHandle != null)
        {
            RendererUserData* rendererUserData = (RendererUserData*)viewport->RendererUserData;

            RendererUserData.Free(rendererUserData);

            viewport->RendererUserData = null;
        }
    }

    private static void ShowWindow(ImGuiViewport* viewport)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        window.Show();
    }

    private static void SetWindowPos(ImGuiViewport* viewport, Vector2 pos)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        window.X = (int)pos.X;
        window.Y = (int)pos.Y;
    }

    private static Vector2* GetWindowPos(Vector2* pos, ImGuiViewport* viewport)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        pos->X = window.X;
        pos->Y = window.Y;

        return pos;
    }

    private static void SetWindowSize(ImGuiViewport* viewport, Vector2 size)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        window.Width = (int)size.X;
        window.Height = (int)size.Y;
    }

    private static Vector2* GetWindowSize(Vector2* size, ImGuiViewport* viewport)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        size->X = window.Width;
        size->Y = window.Height;

        return size;
    }

    private static void SetWindowFocus(ImGuiViewport* viewport)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        window.Focus();
    }

    private static byte GetWindowFocus(ImGuiViewport* viewport)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        return window.Focused ? (byte)1 : (byte)0;
    }

    private static byte GetWindowMinimized(ImGuiViewport* viewport)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        return window.Minimized ? (byte)1 : (byte)0;
    }

    private static void SetWindowTitle(ImGuiViewport* viewport, byte* str)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        window.Title = Marshal.PtrToStringAnsi((nint)str)!;
    }
}
