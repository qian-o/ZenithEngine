using SharpMetal.Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Metal;

internal class MTLBuffer : Buffer
{
    public MTLBuffer(MTLGraphicsContext context, ref readonly BufferDesc desc) : base(context, in desc)
    {
        MTLResourceOptions options = GetResourceOptions(desc.Usage);

        Buffer = Context.Device.NewBuffer(desc.SizeInBytes, options)!;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public SharpMetal.Metal.MTLBuffer Buffer { get; }

    private static MTLResourceOptions GetResourceOptions(BufferUsage usage)
    {
        MTLResourceOptions options = MTLResourceOptions.ResourceStorageModePrivate;

        // Use shared storage for dynamic buffers that need CPU access
        if (usage.HasFlag(BufferUsage.Dynamic))
        {
            options = MTLResourceOptions.ResourceStorageModeShared;

            // Use write-combined CPU cache mode for streaming data
            options |= MTLResourceOptions.ResourceCPUCacheModeWriteCombined;
        }

        // Apply hazard tracking mode optimization
        bool hasUnorderedAccess = usage.HasFlag(BufferUsage.UnorderedAccess);
        bool isDynamic = usage.HasFlag(BufferUsage.Dynamic);
        bool shouldDisableTracking = MTLResourceHelper.ShouldDisableHazardTracking(hasUnorderedAccess, isDynamic, isRenderTarget: false);
        
        if (shouldDisableTracking)
        {
            options |= MTLResourceOptions.ResourceHazardTrackingModeUntracked;
        }

        return options;
    }

    protected override void SetName(string name)
    {
        // Label property is settable in SharpMetal
        Buffer.Label = new NSString(name);
    }

    protected override void Destroy()
    {
        Buffer.Dispose();
    }
}
