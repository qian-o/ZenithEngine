using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using StbiSharp;
using GLTFMaterial = SharpGLTF.Schema2.Material;
using GLTFNode = SharpGLTF.Schema2.Node;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Scene = Renderer.Components.Scene;
using Texture = Graphics.Vulkan.Texture;

namespace Renderer.Scenes;

internal sealed unsafe class GLTFScene(MainWindow mainWindow) : Scene(mainWindow)
{
    #region Structs
    [StructLayout(LayoutKind.Explicit)]
    private struct UBO
    {
        [FieldOffset(0)]
        public Matrix4x4 Projection;

        [FieldOffset(64)]
        public Matrix4x4 View;

        [FieldOffset(128)]
        public Matrix4x4 Model;

        [FieldOffset(192)]
        public Vector4 LightPos;

        [FieldOffset(208)]
        public Vector4 ViewPos;
    }

    private struct Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 color, Vector4 tangent)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector2 TexCoord = texCoord;

        public Vector3 Color = color;

        public Vector4 Tangent = tangent;
    }

    private struct Primitive(uint firstIndex, uint indexCount, int materialIndex)
    {
        public uint FirstIndex = firstIndex;

        public uint IndexCount = indexCount;

        public int MaterialIndex = materialIndex;
    }
    #endregion

    #region Classes
    private sealed class Mesh
    {
        public List<Primitive> Primitives { get; } = [];
    }

    private sealed class Node
    {
        public string Name { get; set; } = string.Empty;

        public Node? Parent { get; set; }

        public List<Node> Children { get; } = [];

        public Mesh? Mesh { get; set; }

        public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;

        public bool IsVisible { get; set; } = true;
    }

    private sealed class Material
    {
        public Vector4 BaseColorFactor { get; set; } = Vector4.One;

        public uint BaseColorTextureIndex { get; set; }

        public uint NormalTextureIndex { get; set; }

        public string AlphaMode { get; set; } = "OPAQUE";

        public float AlphaCutoff { get; set; } = 0.5f;

        public bool DoubleSided { get; set; }
    }
    #endregion

    private readonly List<Texture> _textures = [];
    private readonly List<TextureView> _textureViews = [];
    private readonly List<Material> _materials = [];
    private readonly List<Node> _nodes = [];

    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _uboBuffer = null!;
    private ResourceLayout _uboLayout = null!;
    private ResourceSet _uboSet = null!;
    private ResourceLayout _materialLayout = null!;
    private ResourceSet[] _materialSets = null!;
    private Shader[] _shaders = null!;
    private VertexLayoutDescription[] _vertexLayoutDescriptions = null!;

    private Pipeline[]? _pipelines;

    private Vector3 pos = new(7.8f, 2.1f, 0.0f);

    protected override void Initialize()
    {
        Title = "GLTF Scene";

        string hlsl = File.ReadAllText("Assets/Shaders/GLTF.hlsl");

        ModelRoot root = ModelRoot.Load("Assets/Models/Sponza/glTF/Sponza.gltf");

        foreach (GLTFTexture gltfTexture in root.LogicalTextures)
        {
            Stbi.InfoFromMemory(gltfTexture.PrimaryImage.Content.Content.Span, out int width, out int height, out _);
            StbiImage image = Stbi.LoadFromMemory(gltfTexture.PrimaryImage.Content.Content.Span, 4);

            TextureDescription description = TextureDescription.Texture2D((uint)width, (uint)height, 1, PixelFormat.R8G8B8A8UNorm, TextureUsage.Sampled);

            Texture texture = _resourceFactory.CreateTexture(in description);
            texture.Name = gltfTexture.Name;

            TextureView textureView = _resourceFactory.CreateTextureView(texture);
            textureView.Name = gltfTexture.Name;

            _graphicsDevice.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);

            _textures.Add(texture);
            _textureViews.Add(textureView);

            image.Dispose();
        }

        foreach (GLTFMaterial gltfMaterial in root.LogicalMaterials)
        {
            Material material = new();

            if (gltfMaterial.FindChannel(KnownChannel.BaseColor.ToString()) is MaterialChannel baseColor)
            {
                material.BaseColorFactor = baseColor.Color;

                if (baseColor.Texture != null)
                {
                    material.BaseColorTextureIndex = (uint)baseColor.Texture.LogicalIndex;
                }
            }

            if (gltfMaterial.FindChannel(KnownChannel.Normal.ToString()) is MaterialChannel normal)
            {
                if (normal.Texture != null)
                {
                    material.NormalTextureIndex = (uint)normal.Texture.LogicalIndex;
                }
            }

            material.AlphaMode = gltfMaterial.Alpha.ToString();
            material.AlphaCutoff = gltfMaterial.AlphaCutoff;
            material.DoubleSided = gltfMaterial.DoubleSided;

            _materials.Add(material);
        }

        List<Vertex> vertices = [];
        List<uint> indices = [];

        foreach (GLTFNode gltfNode in root.LogicalNodes)
        {
            LoadNode(gltfNode, null, vertices, indices);
        }

        _vertexBuffer = _resourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(Vertex) * vertices.Count), BufferUsage.VertexBuffer));
        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, new ReadOnlySpan<Vertex>([.. vertices]));

        _indexBuffer = _resourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * indices.Count), BufferUsage.IndexBuffer));
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, new ReadOnlySpan<uint>([.. indices]));

        _uboBuffer = _resourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(UBO), BufferUsage.UniformBuffer));

        ResourceLayoutDescription uboLayoutDescription = new(new ResourceLayoutElementDescription("UBO", ResourceKind.UniformBuffer, ShaderStages.Vertex));
        ResourceLayoutDescription materialLayoutDescription = new(new ResourceLayoutElementDescription("textureColorMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("textureSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("textureNormalMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("normalSampler", ResourceKind.Sampler, ShaderStages.Fragment));

        _uboLayout = _resourceFactory.CreateResourceLayout(in uboLayoutDescription);
        _uboSet = _resourceFactory.CreateResourceSet(new ResourceSetDescription(_uboLayout, _uboBuffer));

        _materialLayout = _resourceFactory.CreateResourceLayout(in materialLayoutDescription);
        _materialSets = new ResourceSet[_materials.Count];
        for (int i = 0; i < _materials.Count; i++)
        {
            ResourceSetDescription materialSetDescription = new(_materialLayout,
                                                                _textureViews[(int)_materials[i].BaseColorTextureIndex],
                                                                _graphicsDevice.LinearSampler,
                                                                _textureViews[(int)_materials[i].NormalTextureIndex],
                                                                _graphicsDevice.LinearSampler);

            _materialSets[i] = _resourceFactory.CreateResourceSet(in materialSetDescription);
        }

        VertexElementDescription positionDescription = new("Position", VertexElementFormat.Float3);
        VertexElementDescription normalDescription = new("Normal", VertexElementFormat.Float3);
        VertexElementDescription texCoordDescription = new("TexCoord", VertexElementFormat.Float2);
        VertexElementDescription colorDescription = new("Color", VertexElementFormat.Float3);
        VertexElementDescription tangentDescription = new("Tangent", VertexElementFormat.Float4);

        _shaders = _resourceFactory.CreateFromSpirv(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(hlsl), "mainVS"),
                                                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(hlsl), "mainPS"));

        _vertexLayoutDescriptions = [new VertexLayoutDescription(positionDescription, normalDescription, texCoordDescription, colorDescription, tangentDescription)];
    }

    protected override void RecreatePipeline(Framebuffer framebuffer)
    {
        if (_pipelines != null)
        {
            foreach (Pipeline pipeline in _pipelines)
            {
                pipeline.Dispose();
            }
        }

        _pipelines = new Pipeline[_materials.Count];
        for (int i = 0; i < _materials.Count; i++)
        {
            bool alphaMask = _materials[i].AlphaMode == "MASK";
            float alphaCutoff = _materials[i].AlphaCutoff;

            GraphicsPipelineDescription pipelineDescription = new()
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = _materials[i].DoubleSided ? RasterizerStateDescription.CullNone : RasterizerStateDescription.Default,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = [_uboLayout, _materialLayout],
                ShaderSet = new ShaderSetDescription(_vertexLayoutDescriptions, _shaders, [new SpecializationConstant(0, alphaMask), new SpecializationConstant(1, alphaCutoff)]),
                Outputs = framebuffer.OutputDescription
            };

            _pipelines[i] = _resourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
        }
    }

    protected override void UpdateCore(UpdateEventArgs e)
    {
        UBO ubo = new()
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, Width / (float)Height, 0.1f, 1000.0f),
            View = Matrix4x4.CreateLookAt(pos, Vector3.Zero, Vector3.UnitY),
            Model = _nodes[0].LocalTransform,
            LightPos = new Vector4(0.0f, 2.5f, 0.0f, 1.0f),
            ViewPos = new Vector4(pos, 1.0f)
        };

        _graphicsDevice.UpdateBuffer(_uboBuffer, 0, [ubo]);
    }

    protected override void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e)
    {
        if (_pipelines == null)
        {
            return;
        }

        commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
        commandList.ClearDepthStencil(1.0f);

        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);

        foreach (Node node in _nodes)
        {
            DrawNode(commandList, node);
        }
    }

    protected override void ImGuiRender()
    {
        ImGui.Begin("Settings");
        {
            ImGui.Text("Camera Position");
            ImGui.SliderFloat("X", ref pos.X, -10.0f, 10.0f);
            ImGui.SliderFloat("Y", ref pos.Y, -10.0f, 10.0f);
            ImGui.SliderFloat("Z", ref pos.Z, -10.0f, 10.0f);
        }
        ImGui.End();
    }

    protected override void Destroy()
    {
        foreach (Texture texture in _textures)
        {
            texture.Dispose();
        }

        base.Destroy();
    }

    private void LoadNode(GLTFNode gltfNode, Node? parent, List<Vertex> vertices, List<uint> indices)
    {
        Node node = new()
        {
            Name = gltfNode.Name,
            LocalTransform = gltfNode.LocalTransform.Matrix
        };

        foreach (GLTFNode children in gltfNode.VisualChildren)
        {
            LoadNode(children, node, vertices, indices);
        }

        if (gltfNode.Mesh != null)
        {
            foreach (MeshPrimitive primitive in gltfNode.Mesh.Primitives)
            {
                uint firsetIndex = (uint)indices.Count;
                uint vertexOffset = (uint)vertices.Count;
                int indexCount = 0;

                // Vertices
                {
                    IList<Vector3>? positionBuffer = null;
                    IList<Vector3>? normalBuffer = null;
                    IList<Vector2>? texCoordBuffer = null;
                    IList<Vector3>? colorBuffer = null;
                    IList<Vector4>? tangentBuffer = null;
                    uint vertexCount = 0;

                    if (primitive.VertexAccessors.TryGetValue("POSITION", out Accessor? positionAccessor))
                    {
                        positionBuffer = positionAccessor.AsVector3Array();
                        vertexCount = (uint)positionAccessor.Count;
                    }

                    if (primitive.VertexAccessors.TryGetValue("NORMAL", out Accessor? normalAccessor))
                    {
                        normalBuffer = normalAccessor.AsVector3Array();
                    }

                    if (primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out Accessor? texCoordAccessor))
                    {
                        texCoordBuffer = texCoordAccessor.AsVector2Array();
                    }

                    if (primitive.VertexAccessors.TryGetValue("COLOR_0", out Accessor? colorAccessor))
                    {
                        colorBuffer = colorAccessor.AsVector3Array();
                    }

                    if (primitive.VertexAccessors.TryGetValue("TANGENT", out Accessor? tangentAccessor))
                    {
                        tangentBuffer = tangentAccessor.AsVector4Array();
                    }

                    for (uint i = 0; i < vertexCount; i++)
                    {
                        Vector3 position = positionBuffer != null ? positionBuffer[(int)i] : Vector3.Zero;
                        Vector3 normal = normalBuffer != null ? normalBuffer[(int)i] : Vector3.Zero;
                        Vector2 texCoord = texCoordBuffer != null ? texCoordBuffer[(int)i] : Vector2.Zero;
                        Vector3 color = colorBuffer != null ? colorBuffer[(int)i] : Vector3.One;
                        Vector4 tangent = tangentBuffer != null ? tangentBuffer[(int)i] : Vector4.Zero;

                        vertices.Add(new Vertex(position, normal, texCoord, color, tangent));
                    }
                }

                // Indices
                {
                    if (primitive.IndexAccessor != null)
                    {
                        indexCount = primitive.IndexAccessor.Count;

                        IList<uint>? indexBuffer = primitive.IndexAccessor.AsIndicesArray();

                        for (int i = 0; i < indexCount; i++)
                        {
                            indices.Add(indexBuffer[i] + vertexOffset);
                        }
                    }
                }

                node.Mesh ??= new();
                node.Mesh.Primitives.Add(new Primitive(firsetIndex, (uint)indexCount, primitive.Material.LogicalIndex));
            }
        }

        if (parent != null)
        {
            node.Parent = parent;
            parent.Children.Add(node);
        }
        else
        {
            _nodes.Add(node);
        }
    }

    private void DrawNode(CommandList commandList, Node node)
    {
        if (!node.IsVisible)
        {
            return;
        }

        if (node.Mesh != null)
        {
            foreach (Primitive primitive in node.Mesh.Primitives)
            {
                commandList.SetPipeline(_pipelines![primitive.MaterialIndex]);
                commandList.SetGraphicsResourceSet(0, _uboSet);
                commandList.SetGraphicsResourceSet(1, _materialSets[primitive.MaterialIndex]);
                commandList.DrawIndexed(primitive.IndexCount, 1, primitive.FirstIndex, 0, 0);
            }
        }

        foreach (Node children in node.Children)
        {
            DrawNode(commandList, children);
        }
    }
}
