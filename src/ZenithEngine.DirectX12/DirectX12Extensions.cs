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

    public static DXCommandProcessor DX(this CommandProcessor processor)
    {
        if (processor is not DXCommandProcessor)
        {
            throw new ZenithEngineException("CommandProcessor is not a DirectX12 command processor.");
        }

        return (DXCommandProcessor)processor;
    }
}
