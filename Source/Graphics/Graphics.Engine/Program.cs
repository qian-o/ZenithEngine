using System.Numerics;
using Graphics.Engine;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Buffer = Graphics.Engine.Buffer;

using Context context = Context.Create(Backend.Vulkan);

context.CreateDevice(true);

BufferDescription bufferDescription = BufferDescription.Create<Vertex>(1024, BufferUsage.VertexBuffer | BufferUsage.Dynamic);

using Buffer buffer = context.Factory.CreateBuffer(in bufferDescription);

TextureDescription textureDescription = TextureDescription.Create2D(1024, 1024, 1);

using Texture texture = context.Factory.CreateTexture(in textureDescription);

TextureViewDescription textureViewDescription = TextureViewDescription.Create(texture);

using TextureView textureView = context.Factory.CreateTextureView(in textureViewDescription);

SamplerDescription samplerDescription = SamplerDescription.Linear;

using Sampler sampler = context.Factory.CreateSampler(in samplerDescription);

FrameBufferDescription frameBufferDescription = FrameBufferDescription.Create(null, texture);

using FrameBuffer frameBuffer = context.Factory.CreateFrameBuffer(in frameBufferDescription);

Console.WriteLine("Initialization completed.");

internal struct Vertex(Vector3 position, Vector4 color)
{
    public Vector3 Position = position;

    public Vector4 Color = color;
}

// 1. Use Dynamic Rendering instead of RenderPass.
// 2. Reduce unnecessary assignment operations.
// 3. Use slang instead of HLSL.