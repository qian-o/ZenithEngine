using System.Numerics;
using Graphics.Engine;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Buffer = Graphics.Engine.Buffer;

using Context context = Context.Create(Backend.Vulkan);

context.CreateDevice(true);

BufferDescription bufferDescription = BufferDescription.Create<Vertex>(1024, BufferUsage.VertexBuffer | BufferUsage.Dynamic);

using Buffer buffer = context.Factory.CreateBuffer(in bufferDescription);

Console.ReadKey();

struct Vertex
{
    public Vector3 Position;

    public Vector4 Color;
}
