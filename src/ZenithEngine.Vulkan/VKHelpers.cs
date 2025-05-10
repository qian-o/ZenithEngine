using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Vulkan;

internal static class VKHelpers
{
    public static uint GetInitialLayers(TextureType type)
    {
        return type is TextureType.TextureCube or TextureType.TextureCubeArray ? 6u : 1u;
    }

    public static uint GetArrayLayers(TextureDesc desc)
    {
        if (desc.Type is TextureType.Texture1DArray or TextureType.Texture2DArray or TextureType.TextureCubeArray)
        {
            return desc.ArrayLayers * GetInitialLayers(desc.Type);
        }

        return GetInitialLayers(desc.Type);
    }

    public static uint GetArrayLayerIndex(TextureDesc desc,
                                          uint mipLevel,
                                          uint arrayLayer,
                                          CubeMapFace face)
    {
        return (mipLevel * GetArrayLayers(desc))
               + (arrayLayer * GetInitialLayers(desc.Type))
               + (uint)face;
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
            throw new ZenithEngineException(ExceptionHelpers.NotSupported(barrier.OldLayout));
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
            throw new ZenithEngineException(ExceptionHelpers.NotSupported(barrier.OldLayout));
        }
    }
}
