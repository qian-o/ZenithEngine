using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.DirectX12;

internal static class DXHelpers
{
    public static ShaderStages[] GraphicsShaderStages { get; } =
    [
        ShaderStages.Vertex,
        ShaderStages.Hull,
        ShaderStages.Domain,
        ShaderStages.Geometry,
        ShaderStages.Pixel
    ];

    public static uint GetInitialLayers(TextureType type)
    {
        return type is TextureType.TextureCube or TextureType.TextureCubeArray ? 6u : 1u;
    }

    public static ushort GetDepthOrArraySize(TextureDesc desc)
    {
        if (desc.Type is TextureType.Texture3D)
        {
            return (ushort)desc.Depth;
        }

        if (desc.Type is TextureType.Texture1DArray or TextureType.Texture2DArray or TextureType.TextureCubeArray)
        {
            return (ushort)(desc.ArrayLayers * GetInitialLayers(desc.Type));
        }

        return (ushort)GetInitialLayers(desc.Type);
    }

    public static uint GetDepthOrArrayIndex(TextureDesc desc,
                                            uint mipLevel,
                                            uint arrayLayer,
                                            CubeMapFace face)
    {
        return (mipLevel * GetDepthOrArraySize(desc))
               + (arrayLayer * GetInitialLayers(desc.Type))
               + (uint)face;
    }
}
