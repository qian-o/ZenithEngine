using SharpMetal.Foundation;
﻿using SharpMetal.Metal;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLDeviceCapabilities(MTLDevice device) : DeviceCapabilities
{
    public override string DeviceName { get; } = device.Name.ToString();

    public override bool IsRayTracingSupported { get; } = device.SupportsRaytracing || device.SupportsRaytracingFromRender;

    public override bool IsMeshShaderSupported { get; } = device.SupportsFamily(MTLGPUFamily.Mac2) || device.SupportsFamily(MTLGPUFamily.Apple7);

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
    public bool SupportsTextureSwizzle { get; } = device.SupportsFamily(MTLGPUFamily.Apple6) || device.SupportsFamily(MTLGPUFamily.Mac2);

    /// <summary>
    /// Gets the maximum framebuffer storage bit depth.
    /// </summary>
    public ulong MaxFramebufferStorageBitDepth { get; } = 0; // Property doesn't exist in SharpMetal 1.1.0

    /// <summary>
    /// Gets a value indicating whether the device supports variable rasterization rate (VRS).
    /// </summary>
    public bool SupportsVariableRasterizationRate { get; } = device.SupportsRasterizationRateMap(1);

    /// <summary>
    /// Gets a value indicating whether the device supports argument buffers.
    /// </summary>
    public bool SupportsArgumentBuffers { get; } = device.SupportsFamily(MTLGPUFamily.Metal4);
}
