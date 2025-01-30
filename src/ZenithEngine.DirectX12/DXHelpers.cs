using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.DirectX12;

internal static class DXHelpers
{
    public static ushort GetDepthOrArraySize(TextureDesc desc)
    {
        if (desc.Type is TextureType.Texture3D)
        {
            return (ushort)desc.Depth;
        }

        uint initialLayers = desc.Type is TextureType.TextureCube or TextureType.TextureCubeArray ? 6u : 1u;

        if (desc.Type is TextureType.Texture1DArray or TextureType.Texture2DArray or TextureType.TextureCubeArray)
        {
            return (ushort)(desc.ArrayLayers * initialLayers);
        }

        return (ushort)initialLayers;
    }

    public static uint GetDepthOrArrayIndex(TextureDesc desc,
                                            uint mipLevel,
                                            uint arrayLayer,
                                            CubeMapFace face)
    {
        uint initialLayers = desc.Type is TextureType.TextureCube or TextureType.TextureCubeArray ? 6u : 1u;

        return (mipLevel * GetDepthOrArraySize(desc)) + (arrayLayer * initialLayers) + (uint)face;
    }
}
