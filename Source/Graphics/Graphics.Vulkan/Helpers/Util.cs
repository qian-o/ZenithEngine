namespace Graphics.Vulkan;

internal static class Util
{
    public static void GetMipDimensions(Texture texture, uint mipLevel, out uint width, out uint height, out uint depth)
    {
        width = GetDimension(texture.Width, mipLevel);
        height = GetDimension(texture.Height, mipLevel);
        depth = GetDimension(texture.Depth, mipLevel);
    }

    public static DeviceBufferRange GetBufferRange(IBindableResource bindableResource, uint additionalOffset)
    {
        if (bindableResource is DeviceBuffer buffer)
        {
            return new DeviceBufferRange(buffer, additionalOffset, buffer.SizeInBytes);
        }

        if (bindableResource is DeviceBufferRange bufferRange)
        {
            return new DeviceBufferRange(bufferRange.Buffer, bufferRange.Offset + additionalOffset, bufferRange.SizeInBytes - additionalOffset);
        }

        throw new InvalidOperationException("Invalid bindable resource type");
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
