using System.Runtime.CompilerServices;
using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Framebuffer : DeviceResource
{
    private readonly RenderPass _renderPass;

    public Framebuffer(GraphicsDevice graphicsDevice, ref readonly FramebufferDescription description, bool isPresented) : base(graphicsDevice)
    {
        bool hasDepth = description.DepthTarget.HasValue;

        uint colorAttachmentCount = (uint)description.ColorTargets.Length;
        uint depthAttachmentCount = hasDepth ? 1u : 0u;
        uint attachmentCount = colorAttachmentCount + depthAttachmentCount;

        AttachmentDescription[] attachments = new AttachmentDescription[attachmentCount];
        AttachmentReference[] references = new AttachmentReference[attachmentCount];

        for (uint i = 0; i < description.ColorTargets.Length; i++)
        {
            Texture colorTarget = description.ColorTargets[i].Target;

            ImageLayout initialLayout = isPresented
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
                InitialLayout = initialLayout,
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

            ImageLayout initialLayout = depthTarget.Usage.HasFlag(TextureUsage.Sampled)
                ? ImageLayout.DepthStencilReadOnlyOptimal
                : ImageLayout.DepthStencilAttachmentOptimal;

            attachments[^1] = new AttachmentDescription
            {
                Format = depthTarget.VkFormat,
                Samples = depthTarget.VkSampleCount,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = hasStencil ? AttachmentStoreOp.Store : AttachmentStoreOp.DontCare,
                InitialLayout = initialLayout,
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

        RenderPass renderPass;
        Vk.CreateRenderPass(graphicsDevice.Device, &createInfo, null, &renderPass).ThrowCode();

        _renderPass = renderPass;
    }

    protected override void Destroy()
    {
        Vk.DestroyRenderPass(GraphicsDevice.Device, _renderPass, null);
    }
}
