using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Vulkan;

internal static class VKHelpers
{
    public static uint GetArrayLayers(TextureDesc desc)
    {
        return desc.Type is TextureType.TextureCube ? 6u : 1u;
    }

    public static uint GetBinding(LayoutElementDesc desc)
    {
        return desc.Type switch
        {
            ResourceType.ConstantBuffer => desc.Slot,

            ResourceType.StructuredBufferReadWrite or
            ResourceType.TextureReadWrite => desc.Slot + 20,

            ResourceType.Sampler => desc.Slot + 40,

            ResourceType.StructuredBuffer or
            ResourceType.Texture or
            ResourceType.AccelerationStructure => desc.Slot + 60,

            _ => 0
        };
    }

    public static void MatchImageLayout(ref ImageMemoryBarrier barrier,
                                        out PipelineStageFlags src,
                                        out PipelineStageFlags dst)
    {
        src = PipelineStageFlags.None;
        dst = PipelineStageFlags.None;

        if (barrier.OldLayout is ImageLayout.Undefined or ImageLayout.Preinitialized)
        {
            barrier.SrcAccessMask = AccessFlags.None;
            src = PipelineStageFlags.TopOfPipeBit;
        }
        else if (barrier.OldLayout is ImageLayout.TransferSrcOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.TransferReadBit;
            src = PipelineStageFlags.TransferBit;
        }
        else if (barrier.OldLayout is ImageLayout.TransferDstOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            src = PipelineStageFlags.TransferBit;
        }
        else if (barrier.OldLayout is ImageLayout.ShaderReadOnlyOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.ShaderReadBit;
            src = PipelineStageFlags.FragmentShaderBit;
        }
        else if (barrier.OldLayout is ImageLayout.General)
        {
            barrier.SrcAccessMask = AccessFlags.ShaderReadBit | AccessFlags.ShaderWriteBit;
            src = PipelineStageFlags.AllGraphicsBit;
        }
        else if (barrier.OldLayout is ImageLayout.ColorAttachmentOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
            src = PipelineStageFlags.ColorAttachmentOutputBit;
        }
        else if (barrier.OldLayout is ImageLayout.DepthStencilAttachmentOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.DepthStencilAttachmentWriteBit;
            src = PipelineStageFlags.LateFragmentTestsBit;
        }
        else if (barrier.OldLayout is ImageLayout.PresentSrcKhr)
        {
            barrier.SrcAccessMask = AccessFlags.MemoryReadBit;
            src = PipelineStageFlags.BottomOfPipeBit;
        }
        else
        {
            throw new InvalidOperationException("Unsupported layout transition.");
        }

        if (barrier.NewLayout is ImageLayout.TransferSrcOptimal)
        {
            barrier.DstAccessMask = AccessFlags.TransferReadBit;
            dst = PipelineStageFlags.TransferBit;
        }
        else if (barrier.NewLayout is ImageLayout.TransferDstOptimal)
        {
            barrier.DstAccessMask = AccessFlags.TransferWriteBit;
            dst = PipelineStageFlags.TransferBit;
        }
        else if (barrier.NewLayout is ImageLayout.ShaderReadOnlyOptimal)
        {
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;
            dst = PipelineStageFlags.FragmentShaderBit;
        }
        else if (barrier.NewLayout is ImageLayout.General)
        {
            barrier.DstAccessMask = AccessFlags.ShaderReadBit | AccessFlags.ShaderWriteBit;
            dst = PipelineStageFlags.AllGraphicsBit;
        }
        else if (barrier.NewLayout is ImageLayout.ColorAttachmentOptimal)
        {
            barrier.DstAccessMask = AccessFlags.ColorAttachmentWriteBit;
            dst = PipelineStageFlags.ColorAttachmentOutputBit;
        }
        else if (barrier.NewLayout is ImageLayout.DepthStencilAttachmentOptimal)
        {
            barrier.DstAccessMask = AccessFlags.DepthStencilAttachmentWriteBit;
            dst = PipelineStageFlags.LateFragmentTestsBit;
        }
        else if (barrier.NewLayout is ImageLayout.PresentSrcKhr)
        {
            barrier.DstAccessMask = AccessFlags.MemoryReadBit;
            dst = PipelineStageFlags.BottomOfPipeBit;
        }
        else
        {
            throw new InvalidOperationException("Unsupported layout transition.");
        }
    }
}
