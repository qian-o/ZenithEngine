using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common;

public class ZenithEngineException(Backend backend, string message) : Exception(message)
{
    public Backend Backend { get; } = backend;
}
