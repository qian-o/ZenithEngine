using Hexa.NET.ImGui;

namespace Graphics.Vulkan;

public record struct ImGuiFontConfig
{
    public ImGuiFontConfig(string fontPath, int fontSize, Func<ImGuiIOPtr, nint>? getGlyphRange = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fontSize);

        FontPath = fontPath;
        FontSize = fontSize;
        GetGlyphRange = getGlyphRange;
    }

    public string FontPath { get; set; }

    public int FontSize { get; set; }

    public Func<ImGuiIOPtr, nint>? GetGlyphRange { get; set; }
}
