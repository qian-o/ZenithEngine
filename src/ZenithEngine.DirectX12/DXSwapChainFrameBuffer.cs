using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXSwapChainFrameBuffer(GraphicsContext context,
                                      DXSwapChain swapChain) : GraphicsResource(context)
{
    private Texture? depthStencilTarget;
    private Texture[] colorTargets = [];
    private FrameBuffer[] frameBuffers = [];

    public FrameBuffer this[uint index] => frameBuffers[index];

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void CreateFrameBuffers(uint width, uint height)
    {
        bool hasDepthStencilAttachment = swapChain.Desc.DepthStencilTargetFormat is not null;

        if (hasDepthStencilAttachment)
        {
            TextureDesc desc = new(width,
                                   height,
                                   format: swapChain.Desc.DepthStencilTargetFormat!.Value,
                                   usage: TextureUsage.DepthStencil);

            depthStencilTarget = Context.Factory.CreateTexture(in desc);
        }

        colorTargets = new Texture[DXSwapChain.BufferCount];
        frameBuffers = new FrameBuffer[DXSwapChain.BufferCount];
        for (uint i = 0; i < DXSwapChain.BufferCount; i++)
        {
            TextureDesc desc = new(width,
                                   height,
                                   format: swapChain.Desc.ColorTargetFormat,
                                   usage: TextureUsage.RenderTarget);

            colorTargets[i] = new DXTexture(Context,
                                            in desc,
                                            swapChain.SwapChain3.GetBuffer<ID3D12Resource>(i));

            FrameBufferDesc frameBufferDesc = new(hasDepthStencilAttachment ? new(depthStencilTarget!) : null, [new(colorTargets[i])]);

            frameBuffers[i] = Context.Factory.CreateFrameBuffer(in frameBufferDesc);
        }
    }

    public void DestroyFrameBuffers()
    {
        foreach (FrameBuffer frameBuffer in frameBuffers)
        {
            frameBuffer.Dispose();
        }

        foreach (Texture colorTarget in colorTargets)
        {
            colorTarget.Dispose();
        }

        depthStencilTarget?.Dispose();

        depthStencilTarget = null;
        colorTargets = [];
        frameBuffers = [];
    }

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        DestroyFrameBuffers();
    }
}
