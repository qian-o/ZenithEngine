using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXFrameBuffer : FrameBuffer
{
    public CpuDescriptorHandle* RtvHandles;
    public CpuDescriptorHandle* DsvHandle;

    public DXFrameBuffer(GraphicsContext context,
                         ref readonly FrameBufferDesc desc) : base(context, in desc)
    {
        ColorAttachmentCount = (uint)desc.ColorTargets.Length;
        HasDepthStencilAttachment = desc.DepthStencilTarget.HasValue;

        RtvHandles = Allocator.Alloc<CpuDescriptorHandle>(ColorAttachmentCount);
        DsvHandle = HasDepthStencilAttachment ? Allocator.Alloc<CpuDescriptorHandle>() : null;

        uint width = 0;
        uint height = 0;
        TextureSampleCount sampleCount = TextureSampleCount.Count1;

        for (uint i = 0; i < ColorAttachmentCount; i++)
        {
            FrameBufferAttachmentDesc attachmentDesc = desc.ColorTargets[i];
            Texture target = attachmentDesc.Target;

            if (i is 0)
            {
                Utils.GetMipDimensions(target.Desc.Width,
                                       target.Desc.Height,
                                       attachmentDesc.MipLevel,
                                       out width,
                                       out height);

                sampleCount = target.Desc.SampleCount;
            }
            else if (target.Desc.SampleCount != sampleCount)
            {
                throw new ZenithEngineException("All targets must have the same sample count.");
            }

            RtvHandles[i] = target.DX().GetRtv(attachmentDesc.MipLevel,
                                               attachmentDesc.ArrayLayer,
                                               attachmentDesc.Face);
        }

        if (HasDepthStencilAttachment)
        {
            FrameBufferAttachmentDesc attachmentDesc = desc.DepthStencilTarget!.Value;
            Texture target = attachmentDesc.Target;

            if (ColorAttachmentCount is 0)
            {
                Utils.GetMipDimensions(target.Desc.Width,
                                       target.Desc.Height,
                                       attachmentDesc.MipLevel,
                                       out width,
                                       out height);

                sampleCount = target.Desc.SampleCount;
            }
            else if (target.Desc.SampleCount != sampleCount)
            {
                throw new ZenithEngineException("All targets must have the same sample count.");
            }

            DsvHandle[0] = target.DX().GetDsv(attachmentDesc.MipLevel,
                                              attachmentDesc.ArrayLayer,
                                              attachmentDesc.Face);
        }

        Width = width;
        Height = height;
        Output = new(sampleCount,
                     HasDepthStencilAttachment ? desc.DepthStencilTarget!.Value.Target.Desc.Format : null,
                     [.. desc.ColorTargets.Select(static item => item.Target.Desc.Format)]);
    }

    public override uint ColorAttachmentCount { get; }

    public override bool HasDepthStencilAttachment { get; }

    public override uint Width { get; }

    public override uint Height { get; }

    public override OutputDesc Output { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void TransitionToIntermediateState(ComPtr<ID3D12GraphicsCommandList> commandList)
    {
        foreach (FrameBufferAttachmentDesc desc in Desc.ColorTargets)
        {
            desc.Target.DX().TransitionState(commandList,
                                             desc.MipLevel,
                                             1,
                                             desc.ArrayLayer,
                                             1,
                                             desc.Face,
                                             1,
                                             ResourceStates.RenderTarget);
        }

        if (Desc.DepthStencilTarget is not null)
        {
            FrameBufferAttachmentDesc desc = Desc.DepthStencilTarget.Value;

            desc.Target.DX().TransitionState(commandList,
                                             desc.MipLevel,
                                             1,
                                             desc.ArrayLayer,
                                             1,
                                             desc.Face,
                                             1,
                                             ResourceStates.DepthWrite);
        }
    }

    public void TransitionToFinalState(ComPtr<ID3D12GraphicsCommandList> commandList)
    {
        foreach (FrameBufferAttachmentDesc desc in Desc.ColorTargets)
        {
            DXTexture texture = desc.Target.DX();

            ResourceStates state;
            if (texture.Desc.Usage.HasFlag(TextureUsage.ShaderResource))
            {
                state = ResourceStates.Common;
            }
            else if (texture.Desc.Usage.HasFlag(TextureUsage.UnorderedAccess))
            {
                state = ResourceStates.UnorderedAccess;
            }
            else if (texture.Desc.Usage.HasFlag(TextureUsage.RenderTarget))
            {
                state = ResourceStates.Present;
            }
            else
            {
                continue;
            }

            texture.TransitionState(commandList,
                                    desc.MipLevel,
                                    1,
                                    desc.ArrayLayer,
                                    1,
                                    desc.Face,
                                    1,
                                    state);
        }

        if (Desc.DepthStencilTarget is not null)
        {
            FrameBufferAttachmentDesc desc = Desc.DepthStencilTarget.Value;

            DXTexture texture = desc.Target.DX();

            if (texture.Desc.Usage.HasFlag(TextureUsage.ShaderResource))
            {
                texture.TransitionState(commandList,
                                        desc.MipLevel,
                                        1,
                                        desc.ArrayLayer,
                                        1,
                                        desc.Face,
                                        1,
                                        ResourceStates.Common);
            }
        }
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        for (uint i = 0; i < ColorAttachmentCount; i++)
        {
            Context.RtvAllocator!.Free(RtvHandles[i]);
        }

        if (HasDepthStencilAttachment)
        {
            Context.DsvAllocator!.Free(DsvHandle[0]);
        }
    }
}
