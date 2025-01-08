namespace ZenithEngine.Common.Graphics;

public struct TextureRegion
{
    public TexturePosition Position;

    public uint Width;

    public uint Height;

    public uint Depth;

    public static TextureRegion Default(uint width,
                                        uint height,
                                        uint depth,
                                        TexturePosition? position = null)
    {
        position ??= TexturePosition.Default();

        return new()
        {
            Position = position.Value,
            Width = width,
            Height = height,
            Depth = depth
        };
    }
}
