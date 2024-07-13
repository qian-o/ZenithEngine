using Hexa.NET.ImGui;

namespace Renderer;

public readonly record struct ImGuiFontConfig
{
    public ImGuiFontConfig(string fontPath, int fontSize, Func<ImGuiIOPtr, nint>? getGlyphRange = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fontSize);

        FontPath = fontPath;
        FontSize = fontSize;
        GetGlyphRange = getGlyphRange;
    }

    public string FontPath { get; init; }

    public int FontSize { get; init; }

    public Func<ImGuiIOPtr, nint>? GetGlyphRange { get; init; }
}
