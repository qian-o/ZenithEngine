using System.Runtime.InteropServices;
using ZenithEngine.Common;

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
}
