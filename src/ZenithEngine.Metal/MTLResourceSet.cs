using System;
using SharpMetal.Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLResourceSet : ResourceSet
{
    private readonly GraphicsResource[] resources;

    public MTLResourceSet(GraphicsContext context,
                          ref readonly ResourceSetDesc desc) : base(context, in desc)
    {
        if (desc.Layout is null)
        {
            throw new ArgumentNullException(nameof(desc.Layout), "Resource layout cannot be null");
        }

        if (desc.Resources is null)
        {
            throw new ArgumentNullException(nameof(desc.Resources), "Resources array cannot be null");
        }

        resources = new GraphicsResource[desc.Resources.Length];
        Array.Copy(desc.Resources, resources, desc.Resources.Length);

        // In Metal, we could create an argument buffer here, but for simplicity
        // we'll just store the resources and bind them directly in the command buffer
        // This is less efficient than argument buffers but simpler to implement
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    /// <summary>
    /// Gets the resources in this set.
    /// </summary>
    public GraphicsResource[] Resources => resources;

    /// <summary>
    /// Binds the resources to the render command encoder.
    /// </summary>
    /// <param name="encoder">The render command encoder.</param>
    /// <param name="slot">The binding slot.</param>
    public void BindToRenderEncoder(MTLRenderCommandEncoder encoder, uint slot)
    {
        MTLResourceLayout layout = (MTLResourceLayout)Desc.Layout;

        uint bufferIndex = slot;
        uint textureIndex = slot;
        uint samplerIndex = slot;

        for (int i = 0; i < resources.Length && i < layout.Desc.Elements.Length; i++)
        {
            ResourceElementDesc element = layout.Desc.Elements[i];
            GraphicsResource resource = resources[i];

            switch (element.Type)
            {
                case ResourceType.ConstantBuffer:
                case ResourceType.StructuredBufferReadOnly:
                case ResourceType.StructuredBufferReadWrite:
                    if (resource is MTLBuffer buffer)
                    {
                        // Bind to both vertex and fragment stages based on shader stages
                        if (element.Stages.HasFlag(ShaderStages.Vertex))
                        {
                            encoder.SetVertexBuffer(buffer.Buffer, 0, bufferIndex);
                        }
                        if (element.Stages.HasFlag(ShaderStages.Fragment))
                        {
                            encoder.SetFragmentBuffer(buffer.Buffer, 0, bufferIndex);
                        }
                        bufferIndex++;
                    }
                    break;

                case ResourceType.Texture:
                case ResourceType.TextureReadWrite:
                    if (resource is MTLTexture texture)
                    {
                        if (element.Stages.HasFlag(ShaderStages.Vertex))
                        {
                            encoder.SetVertexTexture(texture.Texture, textureIndex);
                        }
                        if (element.Stages.HasFlag(ShaderStages.Fragment))
                        {
                            encoder.SetFragmentTexture(texture.Texture, textureIndex);
                        }
                        textureIndex++;
                    }
                    break;

                case ResourceType.Sampler:
                    if (resource is MTLSampler sampler)
                    {
                        if (element.Stages.HasFlag(ShaderStages.Vertex))
                        {
                            encoder.SetVertexSamplerState(sampler.Sampler, samplerIndex);
                        }
                        if (element.Stages.HasFlag(ShaderStages.Fragment))
                        {
                            encoder.SetFragmentSamplerState(sampler.Sampler, samplerIndex);
                        }
                        samplerIndex++;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Binds the resources to the compute command encoder.
    /// </summary>
    /// <param name="encoder">The compute command encoder.</param>
    /// <param name="slot">The binding slot.</param>
    public void BindToComputeEncoder(MTLComputeCommandEncoder encoder, uint slot)
    {
        MTLResourceLayout layout = (MTLResourceLayout)Desc.Layout;

        uint bufferIndex = slot;
        uint textureIndex = slot;
        uint samplerIndex = slot;

        for (int i = 0; i < resources.Length && i < layout.Desc.Elements.Length; i++)
        {
            ResourceElementDesc element = layout.Desc.Elements[i];
            GraphicsResource resource = resources[i];

            if (!element.Stages.HasFlag(ShaderStages.Compute))
            {
                continue;
            }

            switch (element.Type)
            {
                case ResourceType.ConstantBuffer:
                case ResourceType.StructuredBufferReadOnly:
                case ResourceType.StructuredBufferReadWrite:
                    if (resource is MTLBuffer buffer)
                    {
                        encoder.SetBuffer(buffer.Buffer, 0, bufferIndex);
                        bufferIndex++;
                    }
                    break;

                case ResourceType.Texture:
                case ResourceType.TextureReadWrite:
                    if (resource is MTLTexture texture)
                    {
                        encoder.SetTexture(texture.Texture, textureIndex);
                        textureIndex++;
                    }
                    break;

                case ResourceType.Sampler:
                    if (resource is MTLSampler sampler)
                    {
                        encoder.SetSamplerState(sampler.Sampler, samplerIndex);
                        samplerIndex++;
                    }
                    break;
            }
        }
    }

    protected override void SetName(string name)
    {
        // Resource sets don't have a direct Metal object to name
    }

    protected override void Destroy()
    {
        // Resources are owned by the caller, we don't dispose them
    }
}
