namespace Tests.SDFFontTexture;

internal readonly unsafe struct Character(int width, int height, int xoff, int yoff)
{
    public int Width { get; } = width;

    public int Height { get; } = height;

    public byte[] Pixels { get; } = new byte[width * height * 4];

    public int Xoff { get; } = xoff;

    public int Yoff { get; } = yoff;

    public void CopyPixels(byte* bitmap)
    {
        int length = Width * Height;

        for (int i = 0; i < length; i++)
        {
            Pixels[i * 4] = bitmap[i];
            Pixels[(i * 4) + 1] = bitmap[i];
            Pixels[(i * 4) + 2] = bitmap[i];
            Pixels[(i * 4) + 3] = bitmap[i];
        }
    }
}