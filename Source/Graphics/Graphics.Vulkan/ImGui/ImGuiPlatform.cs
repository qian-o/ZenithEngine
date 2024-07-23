using System.Numerics;
using System.Runtime.InteropServices;
using Graphics.Core;
using Hexa.NET.ImGui;

namespace Graphics.Vulkan;

internal unsafe static class ImGuiPlatform
{
    private struct ViewportData
    {
        public GCHandle Window;

        public Window GetWindow() => (Window)Window.Target!;
    }

    public static void Initialize(Window window)
    {
        ImGuiViewport* mainViewport = ImGui.GetMainViewport();
        mainViewport->PlatformHandle = (void*)window.Handle;
        mainViewport->PlatformUserData = AllocViewportData(window);

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

        viewport->PlatformUserData = AllocViewportData(window);
    }

    private static void DestroyWindow(ImGuiViewport* viewport)
    {
        if (viewport->PlatformUserData == null)
        {
            return;
        }

        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        window.Exit();

        FreeViewportData(viewportData);
    }

    private static void ShowWindow(ImGuiViewport* viewport)
    {
        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        window.Show();
    }

    private static void SetWindowPos(ImGuiViewport* viewport, Vector2 pos)
    {
        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        window.X = (int)pos.X;
        window.Y = (int)pos.Y;
    }

    private static Vector2* GetWindowPos(Vector2* pos, ImGuiViewport* viewport)
    {
        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        pos->X = window.X;
        pos->Y = window.Y;

        return pos;
    }

    private static void SetWindowSize(ImGuiViewport* viewport, Vector2 size)
    {
        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        window.Width = (int)size.X;
        window.Height = (int)size.Y;
    }

    private static Vector2* GetWindowSize(Vector2* size, ImGuiViewport* viewport)
    {
        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        size->X = window.Width;
        size->Y = window.Height;

        return size;
    }

    private static void SetWindowFocus(ImGuiViewport* viewport)
    {
        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        window.Focus();
    }

    private static byte GetWindowFocus(ImGuiViewport* viewport)
    {
        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        return window.Focused ? (byte)1 : (byte)0;
    }

    private static byte GetWindowMinimized(ImGuiViewport* viewport)
    {
        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        return window.Minimized ? (byte)1 : (byte)0;
    }

    private static void SetWindowTitle(ImGuiViewport* viewport, byte* str)
    {
        ViewportData* viewportData = (ViewportData*)viewport->PlatformUserData;

        Window window = viewportData->GetWindow();

        window.Title = Marshal.PtrToStringAnsi((nint)str)!;
    }

    private static ViewportData* AllocViewportData(Window window)
    {
        ViewportData* viewportData = (ViewportData*)Marshal.AllocHGlobal(sizeof(ViewportData));
        viewportData->Window = GCHandle.Alloc(window);

        return viewportData;
    }

    private static void FreeViewportData(ViewportData* viewportData)
    {
        Marshal.FreeHGlobal((IntPtr)viewportData);

        viewportData->Window.Free();
    }
}
