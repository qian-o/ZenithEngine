using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.DirectX12;

internal static class DXHelpers
{
    public static ushort GetDepthOrArraySize(TextureDesc desc)
    {
        return desc.Type == TextureType.TextureCube ? (ushort)6 : (ushort)desc.Depth;
    }

    public static uint GetDepthOrArrayIndex(TextureDesc desc, uint mipLevel, CubeMapFace face)
    {
        return (mipLevel * GetDepthOrArraySize(desc)) + (uint)face;
    }
}
