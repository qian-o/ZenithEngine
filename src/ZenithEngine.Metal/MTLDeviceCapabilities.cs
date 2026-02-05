using SharpMetal.Metal;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLDeviceCapabilities(MTLDevice device) : DeviceCapabilities
{
    public override string DeviceName { get; } = device.Name;

    public override bool IsRayTracingSupported { get; } = device.SupportsRaytracing || device.SupportsRaytracingFromRender;

    public override bool IsMeshShaderSupported { get; } = device.SupportsFamily(MTLGpuFamily.Mac2) || device.SupportsFamily(MTLGpuFamily.Apple7);

    /// <summary>
    /// Gets the maximum buffer length supported by the device.
    /// </summary>
    public ulong MaxBufferLength { get; } = device.MaxBufferLength;

    /// <summary>
    /// Gets a value indicating whether the device has unified memory architecture.
    /// </summary>
    public bool HasUnifiedMemory { get; } = device.HasUnifiedMemory;

    /// <summary>
    /// Gets a value indicating whether the device supports counter sampling.
    /// </summary>
    public bool SupportsCounterSampling { get; } = device.SupportsCounterSampling(MTLCounterSamplingPoint.AtStageBoundary);

    /// <summary>
    /// Gets a value indicating whether the device supports texture swizzle.
    /// </summary>
    public bool SupportsTextureSwizzle { get; } = device.SupportsFamily(MTLGpuFamily.Apple6) || device.SupportsFamily(MTLGpuFamily.Mac2);

    /// <summary>
    /// Gets the maximum framebuffer storage bit depth.
    /// </summary>
    public ulong MaxFramebufferStorageBitDepth { get; } = device.MaxFramebufferStorageBitDepth;

    /// <summary>
    /// Gets a value indicating whether the device supports variable rasterization rate (VRS).
    /// </summary>
    public bool SupportsVariableRasterizationRate { get; } = device.SupportsRasterizationRateMap;

    /// <summary>
    /// Gets a value indicating whether the device supports argument buffers.
    /// </summary>
    public bool SupportsArgumentBuffers { get; } = device.SupportsFamily(MTLGpuFamily.Metal4);
}
