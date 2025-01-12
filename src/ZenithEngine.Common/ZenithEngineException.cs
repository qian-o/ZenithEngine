using System.Runtime.CompilerServices;

namespace ZenithEngine.Common;

public class ZenithEngineException(string message) : Exception(message)
{
    public static string NotSupported(object? value, [CallerMemberName] string memberName = "")
    {
        if (value is null)
        {
            return $"The {memberName} has a null value.";
        }
        else
        {
            return $"The {memberName} {value.GetType()} `{value}` is not supported.";
        }
    }
}
