using System.Runtime.InteropServices;
using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal static class DirectX12Extensions
{
    public static void ThrowIfError(this int result)
    {
        if (result is not 0)
        {
            throw new ZenithEngineException($"DirectX12 error code: {result}", Marshal.GetExceptionForHR(result));
        }
    }

    public static DXBuffer DX(this Buffer buffer)
    {
        if (buffer is not DXBuffer)
        {
            throw new ZenithEngineException("Buffer is not a DirectX12 buffer.");
        }

        return (DXBuffer)buffer;
    }

    public static DXTexture DX(this Texture texture)
    {
        if (texture is not DXTexture)
        {
            throw new ZenithEngineException("Texture is not a DirectX12 texture.");
        }

        return (DXTexture)texture;
    }

    public static DXSampler DX(this Sampler sampler)
    {
        if (sampler is not DXSampler)
        {
            throw new ZenithEngineException("Sampler is not a DirectX12 sampler.");
        }

        return (DXSampler)sampler;
    }

    public static DXCommandProcessor DX(this CommandProcessor processor)
    {
        if (processor is not DXCommandProcessor)
        {
            throw new ZenithEngineException("CommandProcessor is not a DirectX12 command processor.");
        }

        return (DXCommandProcessor)processor;
    }
}
