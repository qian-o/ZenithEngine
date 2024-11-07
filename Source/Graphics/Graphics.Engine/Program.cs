using System.Numerics;
using Graphics.Engine;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Helpers;
using Buffer = Graphics.Engine.Buffer;

using Context context = Context.Create(Backend.Vulkan);

context.CreateDevice(true);

BufferDesc bufferDesc = BufferDesc.Default<Vertex>(1024, BufferUsage.VertexBuffer | BufferUsage.Dynamic);

using Buffer buffer = context.Factory.CreateBuffer(in bufferDesc);

TextureDesc textureDesc = TextureDesc.Default2D(1024, 1024, Utils.GetMipLevels(1024, 1024));

using Texture texture = context.Factory.CreateTexture(in textureDesc);

TextureViewDesc textureViewDesc = TextureViewDesc.Default(texture);

using TextureView textureView = context.Factory.CreateTextureView(in textureViewDesc);

SamplerDesc samplerDesc = SamplerDesc.Default(true, 4);

using Sampler sampler = context.Factory.CreateSampler(in samplerDesc);

FrameBufferDesc frameBufferDesc = FrameBufferDesc.Default(null, texture);

using FrameBuffer frameBuffer = context.Factory.CreateFrameBuffer(in frameBufferDesc);

Console.WriteLine("Initialization completed.");

internal struct Vertex(Vector3 position, Vector4 color)
{
    public Vector3 Position = position;

    public Vector4 Color = color;
}

// 1. Use Dynamic Rendering instead of RenderPass.
// 2. Reduce unnecessary assignment operations.
// 3. Use slang instead of HLSL.