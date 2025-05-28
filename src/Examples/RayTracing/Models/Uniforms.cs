using Common;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace RayTracing.Models;

internal class Uniforms : DisposableObject
{
    private readonly GraphicsContext context;

    public Uniforms(GraphicsContext context,
                    TopLevelAS scene,
                    Material[] materials,
                    Vertex[] vertices,
                    uint[] indices,
                    Vector2D<uint>[] offsets,
                    Light[] lights,
                    uint width,
                    uint height)
    {
        this.context = context;

        Scene = scene;
        Globals = new Buffer<Globals>(context, 1, BufferUsage.ConstantBuffer);
        Materials = new Buffer<Material>(context, (uint)materials.Length, BufferUsage.ShaderResource);
        Vertices = new Buffer<Vertex>(context, (uint)vertices.Length, BufferUsage.ShaderResource);
        Indices = new Buffer<uint>(context, (uint)indices.Length, BufferUsage.ShaderResource);
        Offsets = new Buffer<Vector2D<uint>>(context, (uint)offsets.Length, BufferUsage.ShaderResource);
        Lights = new Buffer<Light>(context, (uint)lights.Length, BufferUsage.ShaderResource);

        Materials.CopyFrom(materials);
        Vertices.CopyFrom(vertices);
        Indices.CopyFrom(indices);
        Offsets.CopyFrom(offsets);
        Lights.CopyFrom(lights);

        CreateTextures(width, height);
    }

    public TopLevelAS Scene { get; }

    public Buffer<Globals> Globals { get; }

    public Buffer<Material> Materials { get; }

    public Buffer<Vertex> Vertices { get; }

    public Buffer<uint> Indices { get; }

    public Buffer<Vector2D<uint>> Offsets { get; }

    public Buffer<Light> Lights { get; }

    public Texture Accumulation { get; private set; } = null!;

    public Texture Output { get; private set; } = null!;

    private void CreateTextures(uint width, uint height)
    {
        TextureDesc accumulationDesc = new(width, height, format: PixelFormat.R32G32B32A32Float, usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);
        Accumulation = context.Factory.CreateTexture(in accumulationDesc);

        TextureDesc outputDesc = new(width, height, format: PixelFormat.R8G8B8A8UNorm, usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);
        Output = context.Factory.CreateTexture(in outputDesc);
    }

    protected override void Destroy()
    {
        Scene.Dispose();
        Materials.Dispose();
        Vertices.Dispose();
        Indices.Dispose();
        Offsets.Dispose();
        Globals.Dispose();
        Lights.Dispose();
        Accumulation.Dispose();
        Output.Dispose();
    }
}
