using Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLTexture : Texture
{
    public MTLTexture(GraphicsContext context, ref readonly TextureDesc desc) : base(context, in desc)
    {
        using MTLTextureDescriptor descriptor = new()
        {
            TextureType = MTLFormats.GetMTLTextureType(desc.Type),
            PixelFormat = MTLFormats.GetMTLPixelFormat(desc.Format),
            Width = desc.Width,
            Height = desc.Height,
            Depth = desc.Depth,
            MipmapLevelCount = desc.MipLevels,
            SampleCount = MTLFormats.GetSampleCount(desc.SampleCount),
            ArrayLength = desc.ArrayLayers,
            ResourceOptions = MTLResourceOptions.StorageModePrivate,
            StorageMode = MTLStorageMode.Private,
            AllowGpuOptimizedContents = true,
            Usage = MTLFormats.GetMTLTextureUsage(desc.Usage)
        };

        Texture = Context.Device.CreateTexture(descriptor)!;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public IMTLTexture Texture { get; }

    protected override void SetName(string name)
    {
        Texture.Label = name;
    }

    protected override void Destroy()
    {
        Texture.Dispose();
    }
}
