using Metal;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal unsafe class MTLGraphicsContext : GraphicsContext
{
    public MTLGraphicsContext()
    {
        IMTLDevice device;
        if (MTLDevice.SystemDefault is null)
        {
            throw new InvalidOperationException("No Metal-compatible device found.");
        }
        else
        {
            device = MTLDevice.SystemDefault;
        }

        if (!device.SupportsFamily(MTLGpuFamily.Metal4))
        {
            throw new NotSupportedException(
                $"The selected Metal device '{device.Name}' does not support the Metal 4 feature set required by this engine. " +
                "Metal 4 is available on Apple GPUs starting with A15/M2 and Mac GPUs with macOS 13.0+.");
        }

        Device = device;
        Backend = Backend.Metal;
        Capabilities = new MTLDeviceCapabilities(device);
        Factory = new MTLResourceFactory(this);

        // Create command queues
        GraphicsQueue = device.CreateCommandQueue()!;
        GraphicsQueue.Label = "Graphics Queue";

        ComputeQueue = device.CreateCommandQueue()!;
        ComputeQueue.Label = "Compute Queue";

        CopyQueue = device.CreateCommandQueue()!;
        CopyQueue.Label = "Copy Queue";

        // Log Metal device capabilities for debugging
        LogDeviceCapabilities();
    }

    public IMTLDevice Device { get; }

    public IMTLCommandQueue GraphicsQueue { get; private set; }

    public IMTLCommandQueue ComputeQueue { get; private set; }

    public IMTLCommandQueue CopyQueue { get; private set; }

    public override Backend Backend { get; }

    public override DeviceCapabilities Capabilities { get; }

    public override ResourceFactory Factory { get; }

    public override MappedResource MapMemory(Buffer buffer, MapMode mode)
    {
        throw new NotImplementedException();
    }

    public override void UnmapMemory(Buffer buffer)
    {
        throw new NotImplementedException();
    }

    protected override void CreateDeviceInternal(bool useDebugLayer)
    {
        if (useDebugLayer)
        {
            // Note: Metal's debug layer is enabled via environment variables or Xcode settings,
            // so there's no direct API call to enable it like in DirectX or Vulkan.
            // Set OS_ACTIVITY_MODE=disable and METAL_DEVICE_WRAPPER_TYPE=1 environment variables
            // for Metal validation layer.
        }
    }

    protected override void DestroyInternal()
    {
        GraphicsQueue?.Dispose();
        ComputeQueue?.Dispose();
        CopyQueue?.Dispose();
    }

    private void LogDeviceCapabilities()
    {
        if (Capabilities is not MTLDeviceCapabilities mtlCaps)
        {
            return;
        }

        Console.WriteLine($"Metal Device: {mtlCaps.DeviceName}");
        Console.WriteLine($"  - Ray Tracing: {mtlCaps.IsRayTracingSupported}");
        Console.WriteLine($"  - Mesh Shaders: {mtlCaps.IsMeshShaderSupported}");
        Console.WriteLine($"  - Variable Rasterization Rate: {mtlCaps.SupportsVariableRasterizationRate}");
        Console.WriteLine($"  - Argument Buffers: {mtlCaps.SupportsArgumentBuffers}");
        Console.WriteLine($"  - Counter Sampling: {mtlCaps.SupportsCounterSampling}");
        Console.WriteLine($"  - Texture Swizzle: {mtlCaps.SupportsTextureSwizzle}");
        Console.WriteLine($"  - Unified Memory: {mtlCaps.HasUnifiedMemory}");
        Console.WriteLine($"  - Max Buffer Length: {mtlCaps.MaxBufferLength:N0} bytes");
        Console.WriteLine($"  - Max Framebuffer Bit Depth: {mtlCaps.MaxFramebufferStorageBitDepth}");
    }
}
