namespace ZenithEngine.Common;

public static class ExceptionHelper
{
    public static string NotSupported(object? value)
    {
        if (value is null)
        {
            return $"This null value is not supported.";
        }
        else
        {
            return $"The {value.GetType().Name} - `{value}` is not supported.";
        }
    }
}
