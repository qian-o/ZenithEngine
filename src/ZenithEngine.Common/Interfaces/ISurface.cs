namespace ZenithEngine.Common.Interfaces;

public unsafe interface ISurface
{
    /// <summary>
    /// Create a surface by vulkan
    /// </summary>
    /// <typeparam name="T">Allocator type</typeparam>
    /// <param name="instance">The Vulkan instance to create a surface for.</param>
    /// <param name="allocator">A custom Vulkan allocator. Can be omitted by passing null.</param>
    /// <returns>A handle to the Vulkan surface created</returns>
    nint CreateSurfaceByVulkan<T>(nint instance, T* allocator) where T : unmanaged;
}
