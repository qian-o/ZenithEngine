using System.Numerics;
using System.Runtime.InteropServices;
using Graphics.Core;
using Hexa.NET.ImGui;

namespace Graphics.Vulkan;

internal static unsafe class ImGuiPlatform
{
    public static void Initialize(Window window, GraphicsDevice graphicsDevice)
    {
        ImGuiViewport* mainViewport = ImGui.GetMainViewport();
        mainViewport->PlatformUserData = PlatformUserData.Alloc(new ImGuiWindow(window, graphicsDevice));
        mainViewport->RendererUserData = RendererUserData.Alloc(graphicsDevice);
        mainViewport->PlatformHandle = (void*)window.Handle;

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
        imGuiPlatformIOPtr.PlatformSetWindowAlpha = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowAlpha>(SetWindowAlpha);
    }

    private static void CreateWindow(ImGuiViewport* viewport)
    {
        viewport->PlatformUserData = PlatformUserData.Alloc(new ImGuiWindow(viewport));
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

        if (viewport->RendererUserData != null)
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

    private static void SetWindowAlpha(ImGuiViewport* viewport, float alpha)
    {
        PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

        Window window = platformUserData->GetWindow();

        window.Opacity = alpha;
    }
}
