using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Framebuffer : DeviceResource
{
    private readonly VkRenderPass _renderPassClear;
    private readonly VkRenderPass _renderPassLoad;
    private readonly Texture[] _colors;
    private readonly Texture? _depth;
    private readonly TextureView[] _colorViews;
    private readonly TextureView? _depthView;
    private readonly VkFramebuffer _framebuffer;
    private readonly uint _colorAttachmentCount;
    private readonly uint _depthAttachmentCount;
    private readonly uint _attachmentCount;
    private readonly uint _width;
    private readonly uint _height;
    private readonly OutputDescription _outputDescription;
    private readonly bool _isPresented;

    internal Framebuffer(GraphicsDevice graphicsDevice, ref readonly FramebufferDescription description, bool isPresented) : base(graphicsDevice)
    {
        bool hasDepth = description.DepthTarget.HasValue;

        uint colorAttachmentCount = (uint)description.ColorTargets.Length;
        uint depthAttachmentCount = hasDepth ? 1u : 0u;
        uint attachmentCount = colorAttachmentCount + depthAttachmentCount;

        AttachmentDescription[] attachments = new AttachmentDescription[attachmentCount];
        AttachmentReference[] references = new AttachmentReference[attachmentCount];

        for (uint i = 0; i < colorAttachmentCount; i++)
        {
            Texture colorTarget = description.ColorTargets[i].Target;

            attachments[i] = new AttachmentDescription
            {
                Format = colorTarget.VkFormat,
                Samples = colorTarget.VkSampleCount,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.ColorAttachmentOptimal
            };

            references[i] = new AttachmentReference
            {
                Attachment = i,
                Layout = ImageLayout.ColorAttachmentOptimal
            };
        }

        if (hasDepth)
        {
            Texture depthTarget = description.DepthTarget!.Value.Target;

            bool hasStencil = FormatHelpers.IsStencilFormat(depthTarget.Format);

            attachments[^1] = new AttachmentDescription
            {
                Format = depthTarget.VkFormat,
                Samples = depthTarget.VkSampleCount,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = hasStencil ? AttachmentStoreOp.Store : AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
            };

            references[^1] = new AttachmentReference
            {
                Attachment = attachmentCount - 1,
                Layout = ImageLayout.DepthStencilAttachmentOptimal
            };
        }

        SubpassDescription subpass = new()
        {
            PipelineBindPoint = PipelineBindPoint.Graphics
        };

        if (colorAttachmentCount > 0)
        {
            subpass.ColorAttachmentCount = colorAttachmentCount;
            subpass.PColorAttachments = references.AsPointer();
        }

        if (hasDepth)
        {
            subpass.PDepthStencilAttachment = UnsafeHelpers.AsPointer(ref references[^1]);
        }

        SubpassDependency subpassDependency = new()
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = AccessFlags.None,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstAccessMask = AccessFlags.ColorAttachmentReadBit | AccessFlags.ColorAttachmentWriteBit
        };

        RenderPassCreateInfo createInfo = new()
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = attachmentCount,
            PAttachments = attachments.AsPointer(),
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &subpassDependency
        };

        VkRenderPass renderPassClear;
        Vk.CreateRenderPass(graphicsDevice.Device, &createInfo, null, &renderPassClear).ThrowCode();

        for (uint i = 0; i < colorAttachmentCount; i++)
        {
            attachments[i].LoadOp = AttachmentLoadOp.Load;
            attachments[i].InitialLayout = ImageLayout.ColorAttachmentOptimal;
        }

        if (hasDepth)
        {
            attachments[^1].LoadOp = AttachmentLoadOp.Load;
            attachments[^1].InitialLayout = ImageLayout.DepthStencilAttachmentOptimal;
        }

        VkRenderPass renderPassLoad;
        Vk.CreateRenderPass(graphicsDevice.Device, &createInfo, null, &renderPassLoad).ThrowCode();

        Texture[] colors = new Texture[colorAttachmentCount];
        TextureView[] colorViews = new TextureView[colorAttachmentCount];
        for (int i = 0; i < description.ColorTargets.Length; i++)
        {
            FramebufferAttachmentDescription attachmentDescription = description.ColorTargets[i];

            TextureViewDescription colorDescription = new(attachmentDescription.Target,
                                                          attachmentDescription.MipLevel,
                                                          attachmentDescription.ArrayLayer);

            colors[i] = attachmentDescription.Target;
            colorViews[i] = new TextureView(graphicsDevice, in colorDescription);
        }

        Texture? depth = null;
        TextureView? depthView = null;
        if (hasDepth)
        {
            FramebufferAttachmentDescription attachmentDescription = description.DepthTarget!.Value;

            TextureViewDescription depthDescription = new(attachmentDescription.Target,
                                                          attachmentDescription.MipLevel,
                                                          attachmentDescription.ArrayLayer);

            depth = attachmentDescription.Target;
            depthView = new TextureView(graphicsDevice, in depthDescription);
        }

        VkImageView[] imageViews = new VkImageView[attachmentCount];
        for (int i = 0; i < colorViews.Length; i++)
        {
            imageViews[i] = colorViews[i].Handle;
        }

        if (hasDepth)
        {
            imageViews[^1] = depthView!.Handle;
        }

        FramebufferAttachmentDescription framebufferAttachment;
        if (description.ColorTargets.Length > 0)
        {
            framebufferAttachment = description.ColorTargets[0];
        }
        else
        {
            framebufferAttachment = description.DepthTarget!.Value;
        }

        Util.GetMipDimensions(framebufferAttachment.Target,
                              framebufferAttachment.MipLevel,
                              out uint width,
                              out uint height,
                              out uint _);

        FramebufferCreateInfo framebufferCreateInfo = new()
        {
            SType = StructureType.FramebufferCreateInfo,
            RenderPass = renderPassClear,
            AttachmentCount = attachmentCount,
            PAttachments = imageViews.AsPointer(),
            Width = width,
            Height = height,
            Layers = 1
        };

        VkFramebuffer framebuffer;
        Vk.CreateFramebuffer(graphicsDevice.Device, &framebufferCreateInfo, null, &framebuffer).ThrowCode();

        _renderPassClear = renderPassClear;
        _renderPassLoad = renderPassLoad;
        _colors = colors;
        _depth = depth;
        _colorViews = colorViews;
        _depthView = depthView;
        _framebuffer = framebuffer;
        _colorAttachmentCount = colorAttachmentCount;
        _depthAttachmentCount = depthAttachmentCount;
        _attachmentCount = attachmentCount;
        _width = width;
        _height = height;
        _outputDescription = OutputDescription.CreateFromFramebufferDescription(in description);
        _isPresented = isPresented;
    }

    internal VkFramebuffer Handle => _framebuffer;

    internal VkRenderPass RenderPassClear => _renderPassClear;

    internal VkRenderPass RenderPassLoad => _renderPassLoad;

    internal uint ColorAttachmentCount => _colorAttachmentCount;

    internal uint DepthAttachmentCount => _depthAttachmentCount;

    internal uint AttachmentCount => _attachmentCount;

    public uint Width => _width;

    public uint Height => _height;

    public OutputDescription OutputDescription => _outputDescription;

    internal void TransitionToInitialLayout(CommandBuffer commandBuffer)
    {
        for (int i = 0; i < _colorViews.Length; i++)
        {
            Texture color = _colors[i];

            color.TransitionLayout(commandBuffer, ImageLayout.ColorAttachmentOptimal);
        }

        _depth?.TransitionLayout(commandBuffer, ImageLayout.DepthStencilAttachmentOptimal);
    }

    internal void TransitionToFinalLayout(CommandBuffer commandBuffer)
    {
        for (int i = 0; i < _colorViews.Length; i++)
        {
            Texture color = _colors[i];

            ImageLayout finalLayout = _isPresented
                ? ImageLayout.PresentSrcKhr
                : color.Usage.HasFlag(TextureUsage.Sampled) ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.ColorAttachmentOptimal;

            color.TransitionLayout(commandBuffer, finalLayout);
        }

        if (_depth != null)
        {
            ImageLayout finalLayout = _depth.Usage.HasFlag(TextureUsage.Sampled)
                ? ImageLayout.ShaderReadOnlyOptimal
                : ImageLayout.DepthStencilAttachmentOptimal;

            _depth.TransitionLayout(commandBuffer, finalLayout);
        }
    }

    protected override void Destroy()
    {
        Vk.DestroyFramebuffer(GraphicsDevice.Device, _framebuffer, null);

        _depthView?.Dispose();

        foreach (TextureView colorView in _colorViews)
        {
            colorView.Dispose();
        }

        Vk.DestroyRenderPass(GraphicsDevice.Device, _renderPassLoad, null);
        Vk.DestroyRenderPass(GraphicsDevice.Device, _renderPassClear, null);
    }
}
