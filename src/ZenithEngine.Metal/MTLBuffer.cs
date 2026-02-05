using Metal;
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

    public IMTLBuffer Buffer { get; }

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

        // Disable hazard tracking for buffers that don't need automatic synchronization
        // This is beneficial for compute UAV buffers that manage their own synchronization
        if (usage.HasFlag(BufferUsage.UnorderedAccess) && !usage.HasFlag(BufferUsage.Dynamic))
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
