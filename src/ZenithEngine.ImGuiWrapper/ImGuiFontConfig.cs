using Hexa.NET.ImGui;

namespace ZenithEngine.ImGuiWrapper;

public readonly unsafe struct ImGuiFontConfig(string font,
                                              uint size = 16,
                                              Func<ImGuiIOPtr, nint>? glyphRange = null)
{
    public readonly string Font = font;

    public readonly uint Size = size;

    public readonly Func<ImGuiIOPtr, nint> GlyphRange = glyphRange is not null ? glyphRange : (static io => (nint)io.Fonts.GetGlyphRangesDefault());
}
