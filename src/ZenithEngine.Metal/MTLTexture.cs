using SharpMetal.Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLTexture : Texture
{
    public MTLTexture(GraphicsContext context, ref readonly TextureDesc desc) : base(context, in desc)
    {
        MTLResourceOptions resourceOptions = GetResourceOptions(desc.Usage);
        MTLStorageMode storageMode = GetStorageMode(desc.Usage);

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
            ResourceOptions = resourceOptions,
            StorageMode = storageMode,
            AllowGpuOptimizedContents = ShouldAllowGpuOptimizedContents(desc.Usage, storageMode),
            Usage = MTLFormats.GetMTLTextureUsage(desc.Usage)
        };

        Texture = Context.Device.NewTexture(descriptor)!;
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public SharpMetal.Metal.MTLTexture Texture { get; }

    private static MTLResourceOptions GetResourceOptions(TextureUsage usage)
    {
        // Default to private storage for optimal GPU performance
        MTLResourceOptions options = MTLResourceOptions.ResourceStorageModePrivate;

        // Apply hazard tracking mode optimization
        bool hasUnorderedAccess = usage.HasFlag(TextureUsage.UnorderedAccess);
        bool isRenderTarget = usage.HasFlag(TextureUsage.RenderTarget);
        bool shouldDisableTracking = MTLResourceHelper.ShouldDisableHazardTracking(hasUnorderedAccess, isDynamic: false, isRenderTarget);
        
        if (shouldDisableTracking)
        {
            options |= MTLResourceOptions.ResourceHazardTrackingModeUntracked;
        }

        return options;
    }

    private static MTLStorageMode GetStorageMode(TextureUsage usage)
    {
        // For now, always use private storage mode for best GPU performance
        // In the future, we could use memoryless for depth/stencil-only textures
        return MTLStorageMode.Private;
    }

    private static bool ShouldAllowGpuOptimizedContents(TextureUsage usage, MTLStorageMode storageMode)
    {
        // GPU-optimized contents are beneficial for private storage mode textures
        // that will be used frequently by the GPU
        return storageMode == MTLStorageMode.Private &&
               (usage.HasFlag(TextureUsage.ShaderResource) ||
                usage.HasFlag(TextureUsage.RenderTarget) ||
                usage.HasFlag(TextureUsage.UnorderedAccess));
    }

    protected override void SetName(string name)
    {
        Texture.Label = name;
    }

    protected override void Destroy()
    {
        Texture.Dispose();
    }
}
