using Hexa.NET.ImGui;

namespace ZenithEngine.ImGui;

public readonly unsafe struct ImGuiFontConfig(string font,
                                              uint size = 16,
                                              Func<ImGuiIOPtr, nint>? glyphRange = null)
{
    public string Font { get; } = font;

    public uint Size { get; } = size;

    public Func<ImGuiIOPtr, nint> GlyphRange { get; } = glyphRange is not null ? glyphRange : (static io => (nint)io.Fonts.GetGlyphRangesDefault());
}
