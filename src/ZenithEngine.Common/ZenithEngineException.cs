namespace ZenithEngine.Common;

public class ZenithEngineException(string? message, Exception? innerException) : Exception(message, innerException)
{
    public ZenithEngineException() : this(null, null)
    {
    }

    public ZenithEngineException(string message) : this(message, null)
    {
    }
}
