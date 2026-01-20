using Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Metal;

internal class MTLBuffer : Buffer
{
    public MTLBuffer(MTLGraphicsContext context, ref readonly BufferDesc desc) : base(context, in desc)
    {
        MTLResourceOptions options = MTLResourceOptions.StorageModePrivate;

        if (desc.Usage.HasFlag(BufferUsage.Dynamic))
        {
            options = MTLResourceOptions.StorageModeShared;
        }

        Buffer = Context.Device.CreateBuffer(desc.SizeInBytes, options)!;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public IMTLBuffer Buffer { get; }

    protected override void SetName(string name)
    {
        Buffer.AddDebugMarker(name, new(0, (int)Buffer.Length));
    }

    protected override void Destroy()
    {
        Buffer.Dispose();
    }
}
