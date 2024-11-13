namespace ZenithEngine.Common.Enums;

[Flags]
public enum ElementOptions
{
    None = 0,

    /// <summary>
    /// Create a dynamic binding.
    /// </summary>
    DynamicBinding = 1 << 0
}
