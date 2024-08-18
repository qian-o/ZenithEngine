using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Tests.Core;

namespace Tests.SDFFontTexture;

internal sealed unsafe class MainView : View
{
    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;
    private readonly FontController _fontController;

    private string chars = "";

    public MainView(GraphicsDevice device, ImGuiController imGuiController)
    {
        Title = "SDF Font Texture";

        _device = device;
        _imGuiController = imGuiController;
        _fontController = new FontController(device, "Assets/Fonts/MSYH.TTC", 0, 128);
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
    }

    protected override void OnRender(RenderEventArgs e)
    {
        if (ImGui.Begin("Font Texture"))
        {
            ImGui.InputText("SDF Character", ref chars, 10);

            chars = chars.Trim();

            ImGui.End();
        }

        if (!string.IsNullOrEmpty(chars))
        {
            Texture texture = _fontController.GetTexture(chars[0]);

            ImGui.Image(_imGuiController.GetOrCreateImGuiBinding(_device.ResourceFactory, texture), new Vector2(texture.Width, texture.Height));
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
    }

    protected override void Destroy()
    {
        _fontController.Dispose();
    }
}
