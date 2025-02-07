namespace ZenithEngine.Common.Graphics;

public struct TextureRegion
{
    public TexturePosition Position;

    public uint Width;

    public uint Height;

    public uint Depth;

    public static TextureRegion New(uint width,
                                    uint height,
                                    uint depth,
                                    TexturePosition? position = null)
    {
        position ??= TexturePosition.New();

        return new()
        {
            Position = position.Value,
            Width = width,
            Height = height,
            Depth = depth
        };
    }
}
