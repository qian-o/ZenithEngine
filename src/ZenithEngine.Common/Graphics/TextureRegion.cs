namespace ZenithEngine.Common.Graphics;

public struct TextureRegion(uint width,
                            uint height,
                            uint depth,
                            TexturePosition? position = null)
{
    public TexturePosition Position = position ?? new();

    public uint Width = width;

    public uint Height = height;

    public uint Depth = depth;
}
