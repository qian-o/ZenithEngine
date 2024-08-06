using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Structs;
using SharpGLTF.Schema2;
using StbiSharp;
using GltfAnimation = SharpGLTF.Schema2.Animation;
using GltfTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;

namespace Renderer.Models;

internal sealed unsafe class GLTF : DisposableObject
{
    private readonly Node[] _nodes;
    private readonly Material[] _materials;
    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly Texture[] _textures;
    private readonly TextureView[] _textureViews;

    private GLTF(Node[] nodes, Material[] materials, DeviceBuffer vertexBuffer, DeviceBuffer indexBuffer, Texture[] textures, TextureView[] textureViews)
    {
        _nodes = nodes;
        _materials = materials;
        _vertexBuffer = vertexBuffer;
        _indexBuffer = indexBuffer;
        _textures = textures;
        _textureViews = textureViews;
    }

    public Node[] Nodes => _nodes;

    public Material[] Materials => _materials;

    public DeviceBuffer VertexBuffer => _vertexBuffer;

    public DeviceBuffer IndexBuffer => _indexBuffer;

    public Texture[] Textures => _textures;

    public TextureView[] TextureViews => _textureViews;

    protected override void Destroy()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();

        foreach (Texture texture in _textures)
        {
            texture.Dispose();
        }

        foreach (TextureView textureView in _textureViews)
        {
            textureView.Dispose();
        }
    }

    public static GLTF Load(string path)
    {
        ModelRoot root = ModelRoot.Load(path);

        List<Texture> textures = [];
        List<TextureView> textureViews = [];
        List<Material> materials = [.. root.LogicalMaterials.Select(item => new Material(item))];
        List<Vertex> vertices = [];
        List<uint> indices = [];
        List<Node> nodes = [.. root.LogicalNodes.Select(item => new Node(item, null, vertices, indices))];
        DeviceBuffer vertexBuffer = App.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vertices.Count * sizeof(Vertex)), BufferUsage.VertexBuffer));
        DeviceBuffer indexBuffer = App.ResourceFactory.CreateBuffer(new BufferDescription((uint)(indices.Count * sizeof(uint)), BufferUsage.IndexBuffer));

        // Create texture and texture views
        {
            CommandList commandList = App.ResourceFactory.CreateGraphicsCommandList();

            commandList.Begin();

            foreach (GltfTexture gltfTexture in root.LogicalTextures)
            {
                Stbi.InfoFromMemory(gltfTexture.PrimaryImage.Content.Content.Span, out int width, out int height, out _);

                StbiImage image = Stbi.LoadFromMemory(gltfTexture.PrimaryImage.Content.Content.Span, 4);

                TextureDescription description = TextureDescription.Texture2D((uint)width,
                                                                              (uint)height,
                                                                              MipLevels(width, height),
                                                                              PixelFormat.R8G8B8A8UNorm,
                                                                              TextureUsage.Sampled | TextureUsage.GenerateMipmaps);

                Texture texture = App.ResourceFactory.CreateTexture(in description);
                TextureView textureView = App.ResourceFactory.CreateTextureView(texture);

                commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);
                commandList.GenerateMipmaps(texture);

                textures.Add(texture);
                textureViews.Add(textureView);

                image.Dispose();
            }

            commandList.End();

            App.GraphicsDevice.SubmitCommands(commandList);

            commandList.Dispose();
        }

        // Update vertex and index buffers
        {
            App.GraphicsDevice.UpdateBuffer(vertexBuffer, 0, [.. vertices]);
            App.GraphicsDevice.UpdateBuffer(indexBuffer, 0, [.. indices]);
        }

        // Animation
        {
            foreach (GltfAnimation gltfAnimation in root.LogicalAnimations)
            {

            }
        }

        return new GLTF([.. nodes], [.. materials], vertexBuffer, indexBuffer, [.. textures], [.. textureViews]);
    }

    private static uint MipLevels(int width, int height)
    {
        return (uint)MathF.Floor(MathF.Log2(MathF.Max(width, height))) + 1;
    }
}
