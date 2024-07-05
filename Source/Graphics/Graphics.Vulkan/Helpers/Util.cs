namespace Graphics.Vulkan;

internal static class Util
{
    public static void GetMipDimensions(Texture texture, uint mipLevel, out uint width, out uint height, out uint depth)
    {
        width = GetDimension(texture.Width, mipLevel);
        height = GetDimension(texture.Height, mipLevel);
        depth = GetDimension(texture.Depth, mipLevel);
    }

    private static uint GetDimension(uint largestLevelDimension, uint mipLevel)
    {
        uint ret = largestLevelDimension;
        for (uint i = 0; i < mipLevel; i++)
        {
            ret /= 2;
        }

        return Math.Max(1, ret);
    }
}
