using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using StbImageSharp;
using GLTFMaterial = SharpGLTF.Schema2.Material;
using GLTFNode = SharpGLTF.Schema2.Node;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;

internal sealed unsafe class Program
{
    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    private struct UBO
    {
        public Matrix4x4 Projection;

        public Matrix4x4 View;

        public Matrix4x4 Model;

        public Vector4 LightPos;

        public Vector4 ViewPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 color, Vector4 tangent, int colorMapIndex, int normalMapIndex)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector2 TexCoord = texCoord;

        public Vector3 Color = color;

        public Vector4 Tangent = tangent;

        public int ColorMapIndex = colorMapIndex;

        public int NormalMapIndex = normalMapIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
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

        public Matrix4x4 WorldTransform { get; set; } = Matrix4x4.Identity;

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

    private static GraphicsDevice _device = null!;

    private static readonly List<Texture> _textures = [];
    private static readonly List<TextureView> _textureViews = [];
    private static readonly List<Material> _materials = [];
    private static readonly List<Node> _nodes = [];

    private static DeviceBuffer _vertexBuffer = null!;
    private static DeviceBuffer _indexBuffer = null!;
    private static DeviceBuffer _uboBuffer = null!;
    private static ResourceLayout _uboLayout = null!;
    private static ResourceSet _uboSet = null!;
    private static ResourceLayout _textureMapLayout = null!;
    private static ResourceSet _textureMapSet = null!;
    private static ResourceLayout _textureSamplerLayout = null!;
    private static ResourceSet _textureSamplerSet = null!;
    private static Shader[] _shaders = null!;
    private static VertexLayoutDescription[] _vertexLayoutDescriptions = null!;
    private static Pipeline[] _pipelines = null!;
    private static CommandList _commandList = null!;

    private static UBO _ubo;

    private static void Main(string[] _)
    {
        using SdlWindow window = SdlWindow.CreateWindowByVulkan();
        window.Title = "Tests.GLTFScene";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice(), window);

        _device = device;

        window.Load += Window_Load;
        window.Update += Window_Update;
        window.Render += Window_Render;
        window.Resize += (_, e) => _device.MainSwapchain.Resize(e.Width, e.Height);
        window.Closing += Window_Closing;

        window.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        string hlsl = File.ReadAllText("Assets/Shaders/GLTF.hlsl");

        ModelRoot root = ModelRoot.Load("Assets/Models/Sponza/glTF/Sponza.gltf", new ReadSettings() { Validation = ValidationMode.Skip });

        using CommandList commandList = _device.Factory.CreateGraphicsCommandList();

        commandList.Begin();
        foreach (GLTFTexture gltfTexture in root.LogicalTextures)
        {
            using MemoryStream stream = new(gltfTexture.PrimaryImage.Content.Content.Span.ToArray());

            if (ImageInfo.FromStream(stream) is not ImageInfo imageInfo)
            {
                continue;
            }

            int width = imageInfo.Width;
            int height = imageInfo.Height;

            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            uint mipLevels = Math.Max(1, (uint)MathF.Log2(Math.Max(width, height))) + 1;

            TextureDescription description = TextureDescription.Texture2D((uint)width, (uint)height, mipLevels, PixelFormat.R8G8B8A8UNorm, TextureUsage.Sampled | TextureUsage.GenerateMipmaps);

            Texture texture = _device.Factory.CreateTexture(in description);
            texture.Name = gltfTexture.Name;

            TextureView textureView = _device.Factory.CreateTextureView(texture);
            textureView.Name = gltfTexture.Name;

            commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);
            commandList.GenerateMipmaps(texture);

            _textures.Add(texture);
            _textureViews.Add(textureView);
        }
        commandList.End();

        _device.SubmitCommands(commandList);

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

        _vertexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(vertices.Count, BufferUsage.VertexBuffer));
        _device.UpdateBuffer(_vertexBuffer, [.. vertices]);

        _indexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(indices.Count, BufferUsage.IndexBuffer));
        _device.UpdateBuffer(_indexBuffer, [.. indices]);

        _uboBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<UBO>(1, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        ResourceLayoutDescription uboLayoutDescription = new(new ResourceLayoutElementDescription("ubo", ResourceKind.ConstantBuffer, ShaderStages.Vertex));
        ResourceLayoutDescription textureMapDescription = ResourceLayoutDescription.Bindless((uint)_textureViews.Count,
                                                                                             new ResourceLayoutElementDescription("textureMap", ResourceKind.SampledImage, ShaderStages.Fragment));
        ResourceLayoutDescription textureSamplerDescription = ResourceLayoutDescription.Bindless(2,
                                                                                                 new ResourceLayoutElementDescription("textureSampler", ResourceKind.Sampler, ShaderStages.Fragment));

        _uboLayout = _device.Factory.CreateResourceLayout(in uboLayoutDescription);
        _uboSet = _device.Factory.CreateResourceSet(new ResourceSetDescription(_uboLayout, _uboBuffer));

        _textureMapLayout = _device.Factory.CreateResourceLayout(in textureMapDescription);
        _textureMapSet = _device.Factory.CreateResourceSet(new ResourceSetDescription(_textureMapLayout));
        _textureMapSet.UpdateBindless([.. _textureViews]);

        _textureSamplerLayout = _device.Factory.CreateResourceLayout(in textureSamplerDescription);
        _textureSamplerSet = _device.Factory.CreateResourceSet(new ResourceSetDescription(_textureSamplerLayout));
        _textureSamplerSet.UpdateBindless([_device.Aniso4xSampler, _device.LinearSampler]);

        VertexElementDescription positionDescription = new("Position", VertexElementFormat.Float3);
        VertexElementDescription normalDescription = new("Normal", VertexElementFormat.Float3);
        VertexElementDescription texCoordDescription = new("TexCoord", VertexElementFormat.Float2);
        VertexElementDescription colorDescription = new("Color", VertexElementFormat.Float3);
        VertexElementDescription tangentDescription = new("Tangent", VertexElementFormat.Float4);
        VertexElementDescription colorMapIndexDescription = new("ColorMapIndex", VertexElementFormat.Int1);
        VertexElementDescription normalMapIndexDescription = new("NormalMapIndex", VertexElementFormat.Int1);

        _shaders = _device.Factory.CreateShaderByHLSL(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(hlsl), "mainVS"),
                                                      new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(hlsl), "mainPS"));

        _vertexLayoutDescriptions = [new VertexLayoutDescription(positionDescription,
                                                                 normalDescription,
                                                                 texCoordDescription,
                                                                 colorDescription,
                                                                 tangentDescription,
                                                                 colorMapIndexDescription,
                                                                 normalMapIndexDescription)];

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
                ResourceLayouts = [_uboLayout, _textureMapLayout, _textureSamplerLayout],
                ShaderSet = new ShaderSetDescription(_vertexLayoutDescriptions, _shaders, [new SpecializationConstant(0, alphaMask), new SpecializationConstant(1, alphaCutoff)]),
                Outputs = _device.MainSwapchain.OutputDescription
            };

            _pipelines[i] = _device.Factory.CreateGraphicsPipeline(ref pipelineDescription);
        }

        _commandList = _device.Factory.CreateGraphicsCommandList();
    }

    private static void Window_Update(object? sender, UpdateEventArgs e)
    {
        SdlWindow window = (SdlWindow)sender!;

        _ubo = new()
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, window.FramebufferSize.X / window.FramebufferSize.Y, 0.1f, 1000.0f),
            View = Matrix4x4.CreateLookAt(new Vector3(7.8f, 2.1f, 0.0f), Vector3.Zero, Vector3.UnitY),
            LightPos = Vector4.Transform(new Vector4(0.0f, 2.5f, 0.0f, 1.0f), Matrix4x4.CreateRotationX(MathF.Sin(e.TotalTime))),
            ViewPos = new Vector4(new Vector3(7.8f, 2.1f, 0.0f), 1.0f)
        };
    }

    private static void Window_Render(object? sender, RenderEventArgs e)
    {
        _commandList.Begin();

        _commandList.SetFramebuffer(_device.MainSwapchain.Framebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Black);
        _commandList.ClearDepthStencil(1.0f);

        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);

        foreach (Node node in _nodes)
        {
            DrawNode(_commandList, node);
        }

        _commandList.End();

        _device.SubmitCommandsAndSwapBuffers(_commandList, _device.MainSwapchain);
    }

    private static void Window_Closing(object? sender, ClosingEventArgs e)
    {
        _commandList.Dispose();

        foreach (Pipeline pipeline in _pipelines)
        {
            pipeline.Dispose();
        }

        foreach (Shader shader in _shaders)
        {
            shader.Dispose();
        }

        _textureSamplerSet.Dispose();
        _textureSamplerLayout.Dispose();

        _textureMapSet.Dispose();
        _textureMapLayout.Dispose();

        _uboSet.Dispose();
        _uboLayout.Dispose();

        _uboBuffer.Dispose();
        _indexBuffer.Dispose();
        _vertexBuffer.Dispose();

        foreach (TextureView textureView in _textureViews)
        {
            textureView.Dispose();
        }

        foreach (Texture texture in _textures)
        {
            texture.Dispose();
        }
    }

    private static void LoadNode(GLTFNode gltfNode, Node? parent, List<Vertex> vertices, List<uint> indices)
    {
        Node node = new()
        {
            Name = gltfNode.Name,
            LocalTransform = gltfNode.LocalTransform.Matrix,
            WorldTransform = gltfNode.WorldMatrix
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
                        int colorMapIndex = (int)_materials[primitive.Material.LogicalIndex].BaseColorTextureIndex;
                        int normalMapIndex = (int)_materials[primitive.Material.LogicalIndex].NormalTextureIndex;

                        vertices.Add(new Vertex(position,
                                                normal,
                                                texCoord,
                                                color,
                                                tangent,
                                                colorMapIndex,
                                                normalMapIndex));
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

    private static void DrawNode(CommandList commandList, Node node)
    {
        if (!node.IsVisible)
        {
            return;
        }

        if (node.Mesh != null)
        {
            _ubo.Model = node.WorldTransform;

            commandList.UpdateBuffer(_uboBuffer, ref _ubo);

            foreach (Primitive primitive in node.Mesh.Primitives)
            {
                commandList.SetPipeline(_pipelines![primitive.MaterialIndex]);
                commandList.SetResourceSet(0, _uboSet);
                commandList.SetResourceSet(1, _textureMapSet);
                commandList.SetResourceSet(2, _textureSamplerSet);
                commandList.DrawIndexed(primitive.IndexCount, 1, primitive.FirstIndex, 0, 0);
            }
        }

        foreach (Node children in node.Children)
        {
            DrawNode(commandList, children);
        }
    }
}