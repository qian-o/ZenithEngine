using SharpMetal.Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Metal;

internal class MTLBuffer : Buffer
{
    public MTLBuffer(MTLGraphicsContext context, ref readonly BufferDesc desc) : base(context, in desc)
    {
        MTLResourceOptions options = GetResourceOptions(desc.Usage);

        Buffer = Context.Device.CreateBuffer(desc.SizeInBytes, options)!;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public MTLBuffer Buffer { get; }

    private static MTLResourceOptions GetResourceOptions(BufferUsage usage)
    {
        MTLResourceOptions options = MTLResourceOptions.StorageModePrivate;

        // Use shared storage for dynamic buffers that need CPU access
        if (usage.HasFlag(BufferUsage.Dynamic))
        {
            options = MTLResourceOptions.StorageModeShared;

            // Use write-combined CPU cache mode for streaming data
            options |= MTLResourceOptions.CPUCacheModeWriteCombined;
        }

        // Apply hazard tracking mode optimization
        bool hasUnorderedAccess = usage.HasFlag(BufferUsage.UnorderedAccess);
        bool isDynamic = usage.HasFlag(BufferUsage.Dynamic);
        bool shouldDisableTracking = MTLResourceHelper.ShouldDisableHazardTracking(hasUnorderedAccess, isDynamic, isRenderTarget: false);
        
        if (shouldDisableTracking)
        {
            options |= MTLResourceOptions.HazardTrackingModeUntracked;
        }

        return options;
    }

    protected override void SetName(string name)
    {
        Buffer.Label = name;
    }

    protected override void Destroy()
    {
        Buffer.Dispose();
    }
}
