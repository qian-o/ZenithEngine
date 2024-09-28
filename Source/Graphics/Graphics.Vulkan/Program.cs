using System.Numerics;
using Graphics.Core;

namespace Graphics.Vulkan;

internal sealed class Program
{
    private struct Vertex(Vector3 position, Vector3 normal, Vector3 color, Vector2 texCoord)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector3 Color = color;

        public Vector2 TexCoord = texCoord;
    }

    private static GraphicsDevice _device = null!;
    private static ImGuiController _imGuiController = null!;
    private static CommandList _commandList = null!;

    private static void Main(string[] _)
    {
        using Window window = Window.CreateWindowByVulkan();
        window.Title = "Graphics.Vulkan";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice(), window);
        using ImGuiController imGuiController = new(window, device);
        using CommandList commandList = device.Factory.CreateGraphicsCommandList();

        window.Load += Load;
        window.Update += Update;
        window.Render += Render;
        window.Resize += Resize;

        _device = device;
        _imGuiController = imGuiController;
        _commandList = commandList;

        window.Run();
    }

    private static void Load(object? sender, LoadEventArgs e)
    {
    }

    private static void Update(object? sender, UpdateEventArgs e)
    {
    }

    private static void Render(object? sender, RenderEventArgs e)
    {
    }

    private static void Resize(object? sender, ResizeEventArgs e)
    {
    }
}
