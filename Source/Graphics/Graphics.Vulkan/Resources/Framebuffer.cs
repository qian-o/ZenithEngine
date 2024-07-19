using System.Runtime.CompilerServices;
using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Framebuffer : DeviceResource
{
    private readonly VkRenderPass _renderPass;
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

            ImageLayout finalLayout = isPresented
                ? ImageLayout.PresentSrcKhr
                : colorTarget.Usage.HasFlag(TextureUsage.Sampled) ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.ColorAttachmentOptimal;

            attachments[i] = new AttachmentDescription
            {
                Format = colorTarget.VkFormat,
                Samples = colorTarget.VkSampleCount,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = finalLayout
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

            ImageLayout finalLayout = depthTarget.Usage.HasFlag(TextureUsage.Sampled)
                ? ImageLayout.DepthStencilReadOnlyOptimal
                : ImageLayout.DepthStencilAttachmentOptimal;

            attachments[^1] = new AttachmentDescription
            {
                Format = depthTarget.VkFormat,
                Samples = depthTarget.VkSampleCount,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = hasStencil ? AttachmentStoreOp.Store : AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = finalLayout
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
            subpass.PColorAttachments = (AttachmentReference*)Unsafe.AsPointer(ref references[0]);
        }

        if (hasDepth)
        {
            subpass.PDepthStencilAttachment = (AttachmentReference*)Unsafe.AsPointer(ref references[^1]);
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
            PAttachments = (AttachmentDescription*)Unsafe.AsPointer(ref attachments[0]),
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &subpassDependency
        };

        VkRenderPass renderPass;
        Vk.CreateRenderPass(graphicsDevice.Device, &createInfo, null, &renderPass).ThrowCode();

        Texture[] colors = new Texture[description.ColorTargets.Length];
        TextureView[] colorViews = new TextureView[description.ColorTargets.Length];
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
            RenderPass = renderPass,
            AttachmentCount = attachmentCount,
            PAttachments = (VkImageView*)Unsafe.AsPointer(ref imageViews[0]),
            Width = width,
            Height = height,
            Layers = 1
        };

        VkFramebuffer framebuffer;
        Vk.CreateFramebuffer(graphicsDevice.Device, &framebufferCreateInfo, null, &framebuffer).ThrowCode();

        _renderPass = renderPass;
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
        _isPresented = isPresented;
    }

    internal VkFramebuffer Handle => _framebuffer;

    internal VkRenderPass RenderPass => _renderPass;

    internal uint ColorAttachmentCount => _colorAttachmentCount;

    internal uint DepthAttachmentCount => _depthAttachmentCount;

    internal uint AttachmentCount => _attachmentCount;

    public uint Width => _width;

    public uint Height => _height;

    public bool IsPresented => _isPresented;

    public void TransitionToInitialLayout(CommandBuffer commandBuffer)
    {
        for (int i = 0; i < _colors.Length; i++)
        {
            _colors[i].TransitionLayout(commandBuffer, ImageLayout.ColorAttachmentOptimal);
        }

        _depth?.TransitionLayout(commandBuffer, ImageLayout.DepthStencilAttachmentOptimal);
    }

    public void TransitionToFinalLayout(CommandBuffer commandBuffer)
    {
        ImageLayout finalLayout = _isPresented ? ImageLayout.PresentSrcKhr : ImageLayout.ShaderReadOnlyOptimal;

        for (int i = 0; i < _colors.Length; i++)
        {
            _colors[i].TransitionLayout(commandBuffer, finalLayout);
        }

        _depth?.TransitionLayout(commandBuffer, ImageLayout.ShaderReadOnlyOptimal);
    }

    protected override void Destroy()
    {
        Vk.DestroyFramebuffer(GraphicsDevice.Device, _framebuffer, null);

        _depthView?.Dispose();

        foreach (TextureView colorView in _colorViews)
        {
            colorView.Dispose();
        }

        Vk.DestroyRenderPass(GraphicsDevice.Device, _renderPass, null);
    }
}
