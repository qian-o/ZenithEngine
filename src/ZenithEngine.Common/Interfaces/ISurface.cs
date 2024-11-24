namespace ZenithEngine.Common.Interfaces;

public unsafe interface ISurface
{
    /// <summary>
    /// Create a surface by vulkan
    /// </summary>
    /// <typeparam name="T">Allocator type</typeparam>
    /// <param name="instance">The Vulkan instance to create a surface for.</param>
    /// <returns>A handle to the Vulkan surface created</returns>
    ulong CreateSurfaceByVulkan(nint instance);
}
