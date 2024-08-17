using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Tests.Core;

namespace Tests.SDFFontTexture;

internal sealed unsafe class MainView : View
{
    private readonly GraphicsDevice _device;
    private readonly FontController _fontController;
    private readonly Texture _fontTexture;
    private readonly nint _fontTextureId;

    private string chars = "";

    public MainView(GraphicsDevice device, ImGuiController imGuiController)
    {
        Title = "SDF Font Texture";

        _device = device;
        _fontController = new FontController("Assets/Fonts/MSYH.TTC", 0, 64);
        _fontTexture = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(128, 128, 1, PixelFormat.B8G8R8A8UNorm, TextureUsage.Sampled));
        _fontTextureId = imGuiController.GetOrCreateImGuiBinding(device.ResourceFactory, _fontTexture);
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
    }

    protected override void OnRender(RenderEventArgs e)
    {
        if (ImGui.Begin("Font Texture"))
        {
            if (ImGui.InputText("SDF Character", ref chars, 10) && !string.IsNullOrEmpty(chars))
            {
                Character character = _fontController.GetCharacter(chars[0]);

                _device.UpdateTexture(_fontTexture, character.Pixels, 0, 0, 0, (uint)character.Width, (uint)character.Height, 1, 0, 0);
            }

            ImGui.End();
        }


        ImGui.Image(_fontTextureId, new Vector2(_fontTexture.Width, _fontTexture.Height));
    }

    protected override void OnResize(ResizeEventArgs e)
    {
    }

    protected override void Destroy()
    {
        _fontTexture.Dispose();
    }
}
