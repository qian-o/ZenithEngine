using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Graphics.Vulkan.ImGui;
using Hexa.NET.ImGui;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using StbImageSharp;
using Tests.Core;
using Tests.Core.Helpers;
using AlphaMode = SharpGLTF.Schema2.AlphaMode;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;

namespace Tests.RayQuery;

internal sealed unsafe class MainView : View
{
    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        public Vector3 Position;

        public Vector3 Normal;

        public Vector2 TexCoord;

        public Vector3 Color;

        public Vector4 Tangent;

        public uint NodeIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GeometryNode
    {
        public Matrix4x4 Transform;

        public bool AlphaMask;

        public float AlphaCutoff;

        public bool DoubleSided;

        public Vector4 BaseColorFactor;

        public uint BaseColorTextureIndex;

        public uint NormalTextureIndex;

        public uint RoughnessTextureIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Camera
    {
        public Vector3 Position;

        public Vector3 Forward;

        public Vector3 Right;

        public Vector3 Up;

        public float NearPlane;

        public float FarPlane;

        public float Fov;

        public Matrix4x4 ViewMatrix;

        public Matrix4x4 ProjectionMatrix;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Light
    {
        public Vector3 Position;

        public Vector4 AmbientColor;

        public Vector4 DiffuseColor;
    };
    #endregion

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;
    private readonly ViewController _viewController;
    private readonly CameraController _cameraController;

    private readonly List<Texture> _textures;
    private readonly List<TextureView> _textureViews;
    private readonly List<Vertex> _vertices;
    private readonly List<uint> _indices;
    private readonly List<GeometryNode> _nodes;
    private readonly List<AccelStructTriangles> _triangles;
    private readonly List<Light> _lights;

    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly DeviceBuffer _nodeBuffer;
    private readonly DeviceBuffer _cameraBuffer;
    private readonly DeviceBuffer _lightBuffer;
    private readonly BottomLevelAS _bottomLevel;
    private readonly TopLevelAS _topLevel;
    private readonly ResourceLayout _resourceLayout0;
    private readonly ResourceSet _resourceSet0;
    private readonly ResourceLayout _resourceLayout1;
    private readonly ResourceSet _resourceSet1;
    private readonly ResourceLayout _resourceLayout2;
    private readonly ResourceSet _resourceSet2;
    private readonly VertexLayoutDescription _vertexLayout;
    private readonly Shader[] _shaders;
    private readonly CommandList _commandList;

    private FramebufferObject? _framebufferObject;
    private Pipeline? _pipeline;

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("Ray Query")
    {
        _device = device;
        _imGuiController = imGuiController;
        _viewController = new ViewController(this);
        _cameraController = new CameraController(_viewController);
        _cameraController.Transform(Matrix4x4.CreateRotationY(90.0f.ToRadians()) * Matrix4x4.CreateTranslation(new Vector3(0.0f, 1.2f, 0.0f)));

        _textures = [];
        _textureViews = [];
        _vertices = [];
        _indices = [];
        _nodes = [];
        _triangles = [];
        _lights = [];

        LoadGLTF("Assets/Models/Sponza/glTF/Sponza.gltf");

        _lights.Add(new Light()
        {
            Position = new Vector3(-100.0f, 100.0f, 0.0f),
            AmbientColor = new Vector4(0.05f, 0.05f, 0.05f, 1.0f),
            DiffuseColor = new Vector4(1.0f, 0.9f, 0.7f, 1.0f)
        });

        _vertexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(_vertices.Count, BufferUsage.VertexBuffer | BufferUsage.AccelerationStructure));
        _device.UpdateBuffer(_vertexBuffer, [.. _vertices]);

        _indexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(_indices.Count, BufferUsage.IndexBuffer | BufferUsage.AccelerationStructure));
        _device.UpdateBuffer(_indexBuffer, [.. _indices]);

        _nodeBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<GeometryNode>(_nodes.Count, BufferUsage.StorageBuffer));
        _device.UpdateBuffer(_nodeBuffer, [.. _nodes]);

        _cameraBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Camera>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        _lightBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Light>(_lights.Count, BufferUsage.StorageBuffer));
        _device.UpdateBuffer(_lightBuffer, [.. _lights]);

        for (int i = 0; i < _triangles.Count; i++)
        {
            _triangles[i].VertexBuffer = _vertexBuffer;
            _triangles[i].IndexBuffer = _indexBuffer;
        }

        BottomLevelASDescription bottomLevelASDescription = new()
        {
            Geometries = [.. _triangles]
        };

        _bottomLevel = device.Factory.CreateBottomLevelAS(in bottomLevelASDescription);

        AccelStructInstance instance = new()
        {
            Transform4x4 = Matrix4x4.Identity,
            InstanceID = 0,
            InstanceMask = 0xFF,
            InstanceContributionToHitGroupIndex = 0,
            Options = AccelStructInstanceOptions.TriangleCullDisable,
            BottomLevel = _bottomLevel
        };

        TopLevelASDescription topLevelASDescription = new()
        {
            Instances = [instance],
            Options = AccelStructBuildOptions.PreferFastTrace
        };

        _topLevel = device.Factory.CreateTopLevelAS(in topLevelASDescription);

        ElementDescription[] elements0 =
        [
            new ElementDescription("as", ResourceKind.AccelerationStructure, ShaderStages.Pixel),
            new ElementDescription("geometryNodes", ResourceKind.StorageBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("camera", ResourceKind.ConstantBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("lights", ResourceKind.StorageBuffer, ShaderStages.Pixel)
        ];

        _resourceLayout0 = device.Factory.CreateResourceLayout(new ResourceLayoutDescription(elements0));
        _resourceSet0 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout0,
                                                                                    _topLevel,
                                                                                    _nodeBuffer,
                                                                                    _cameraBuffer,
                                                                                    _lightBuffer));

        ElementDescription[] elements1 =
        [
            new ElementDescription("textureArray", ResourceKind.SampledImage, ShaderStages.Pixel)
        ];

        _resourceLayout1 = device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_textureViews.Count, elements1));
        _resourceSet1 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout1));
        _resourceSet1.UpdateBindless([.. _textureViews]);

        ElementDescription[] elements2 =
        [
            new ElementDescription("samplerArray", ResourceKind.Sampler, ShaderStages.Pixel)
        ];

        _resourceLayout2 = device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless(2, elements2));
        _resourceSet2 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout2));
        _resourceSet2.UpdateBindless([_device.Aniso4xSampler, _device.LinearSampler]);

        VertexElementDescription positionElement = new(nameof(Vertex.Position), VertexElementFormat.Float3);
        VertexElementDescription normalElement = new(nameof(Vertex.Normal), VertexElementFormat.Float3);
        VertexElementDescription texCoordElement = new(nameof(Vertex.TexCoord), VertexElementFormat.Float2);
        VertexElementDescription colorElement = new(nameof(Vertex.Color), VertexElementFormat.Float3);
        VertexElementDescription tangentElement = new(nameof(Vertex.Tangent), VertexElementFormat.Float4);
        VertexElementDescription nodeIndexElement = new(nameof(Vertex.NodeIndex), VertexElementFormat.UInt1);

        _vertexLayout = new(positionElement, normalElement, texCoordElement, colorElement, tangentElement, nodeIndexElement);

        string hlsl = File.ReadAllText("Assets/Shaders/rayQuery.slang");

        _shaders = _device.Factory.CreateShaderByHLSL(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(hlsl), "mainVS"),
                                                      new ShaderDescription(ShaderStages.Pixel, Encoding.UTF8.GetBytes(hlsl), "mainPS"));

        _commandList = _device.Factory.CreateGraphicsCommandList();
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
        _viewController.Update();
        _cameraController.Update(e.DeltaTime);

        Camera camera = new()
        {
            Position = _cameraController.Position,
            Forward = _cameraController.Forward,
            Right = _cameraController.Right,
            Up = _cameraController.Up,
            NearPlane = _cameraController.NearPlane,
            FarPlane = _cameraController.FarPlane,
            Fov = _cameraController.Fov.ToRadians(),
            ViewMatrix = _cameraController.ViewMatrix,
            ProjectionMatrix = _cameraController.ProjectionMatrix(Width, Height)
        };

        _device.UpdateBuffer(_cameraBuffer, in camera);
    }

    protected override void OnRender(RenderEventArgs e)
    {
        if (_framebufferObject != null)
        {
            _commandList.Begin();

            _commandList.SetFramebuffer(_framebufferObject.Framebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1.0f);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);
            _commandList.SetPipeline(_pipeline!);
            _commandList.SetResourceSet(0, _resourceSet0);
            _commandList.SetResourceSet(1, _resourceSet1);
            _commandList.SetResourceSet(2, _resourceSet2);

            _commandList.DrawIndexed((uint)_indices.Count, 1, 0, 0, 0);

            _framebufferObject.Present(_commandList);

            _commandList.End();

            _device.SubmitCommands(_commandList);

            ImGui.Image(_imGuiController.GetBinding(_device.Factory, _framebufferObject.PresentTexture), new Vector2(_framebufferObject.Width, _framebufferObject.Height));
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (_framebufferObject != null)
        {
            _imGuiController.RemoveBinding(_imGuiController.GetBinding(_device.Factory, _framebufferObject.PresentTexture));

            _framebufferObject.Dispose();
        }
        _framebufferObject = new FramebufferObject(_device, (int)e.Width, (int)e.Height);

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.CullNone,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_resourceLayout0, _resourceLayout1, _resourceLayout2],
            Shaders = new GraphicsShaderDescription([_vertexLayout], _shaders),
            Outputs = _framebufferObject.Framebuffer.OutputDescription
        };

        _pipeline?.Dispose();
        _pipeline = _device.Factory.CreateGraphicsPipeline(pipelineDescription);
    }

    protected override void Destroy()
    {
        _pipeline?.Dispose();
        _framebufferObject?.Dispose();

        _commandList.Dispose();

        foreach (Shader shader in _shaders)
        {
            shader.Dispose();
        }

        _resourceSet2.Dispose();
        _resourceLayout2.Dispose();

        _resourceSet1.Dispose();
        _resourceLayout1.Dispose();

        _resourceSet0.Dispose();
        _resourceLayout0.Dispose();

        _topLevel.Dispose();
        _bottomLevel.Dispose();

        _lightBuffer.Dispose();
        _cameraBuffer.Dispose();
        _nodeBuffer.Dispose();
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

    private void LoadGLTF(string path)
    {
        ModelRoot root = ModelRoot.Load(path, new ReadSettings() { Validation = ValidationMode.Skip });

        #region Load Textures
        using CommandList commandList = _device.Factory.CreateGraphicsCommandList();

        commandList.Begin();

        foreach (GLTFTexture gltfTexture in root.LogicalTextures)
        {
            using Stream stream = gltfTexture.PrimaryImage.Content.Open();

            if (ImageInfo.FromStream(stream) is not ImageInfo imageInfo)
            {
                continue;
            }

            int width = imageInfo.Width;
            int height = imageInfo.Height;

            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            uint mipLevels = Math.Max(1, (uint)MathF.Log2(Math.Max(width, height))) + 1;

            TextureDescription description = TextureDescription.Texture2D((uint)width,
                                                                          (uint)height,
                                                                          mipLevels,
                                                                          PixelFormat.R8G8B8A8UNorm,
                                                                          TextureUsage.Sampled | TextureUsage.GenerateMipmaps);

            Texture texture = _device.Factory.CreateTexture(in description);
            texture.Name = gltfTexture.Name;

            TextureView textureView = _device.Factory.CreateTextureView(texture);
            textureView.Name = gltfTexture.Name;

            commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);
            commandList.GenerateMipmaps(texture);

            _textures.Add(texture);
            _textureViews.Add(textureView);

            gltfTexture.ClearImages();
        }

        commandList.End();

        _device.SubmitCommands(commandList);
        #endregion

        #region Load Meshes
        foreach (Node node in root.LogicalNodes)
        {
            LoadNode(node);
        }
        #endregion
    }

    private void LoadNode(Node node)
    {
        foreach (Node children in node.VisualChildren)
        {
            LoadNode(children);
        }

        if (node.Mesh == null)
        {
            return;
        }

        foreach (MeshPrimitive primitive in node.Mesh.Primitives)
        {
            List<Vertex> vertices = [];
            List<uint> indices = [];

            uint vertexCount = 0;

            IList<Vector3>? positionBuffer = null;
            IList<Vector3>? normalBuffer = null;
            IList<Vector2>? texCoordBuffer = null;
            IList<Vector3>? colorBuffer = null;
            IList<Vector4>? tangentBuffer = null;

            if (primitive.VertexAccessors.TryGetValue("POSITION", out Accessor? positionAccessor))
            {
                vertexCount = (uint)positionAccessor.Count;

                positionBuffer = positionAccessor.AsVector3Array();
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

            if (vertexCount == 0)
            {
                continue;
            }

            for (uint i = 0; i < vertexCount; i++)
            {
                Vector3 position = positionBuffer != null ? positionBuffer[(int)i] : Vector3.Zero;
                Vector3 normal = normalBuffer != null ? normalBuffer[(int)i] : Vector3.Zero;
                Vector2 texCoord = texCoordBuffer != null ? texCoordBuffer[(int)i] : Vector2.Zero;
                Vector3 color = colorBuffer != null ? colorBuffer[(int)i] : Vector3.One;
                Vector4 tangent = tangentBuffer != null ? tangentBuffer[(int)i] : Vector4.Zero;

                vertices.Add(new Vertex()
                {
                    Position = position,
                    Normal = normal,
                    TexCoord = texCoord,
                    Color = color,
                    Tangent = tangent,
                    NodeIndex = (uint)_nodes.Count
                });
            }

            if (primitive.IndexAccessor != null)
            {
                indices.AddRange(primitive.IndexAccessor.AsIndicesArray());
            }

            GeometryNode geometryNode = new()
            {
                Transform = node.WorldMatrix,
                AlphaMask = primitive.Material.Alpha == AlphaMode.MASK,
                AlphaCutoff = primitive.Material.AlphaCutoff,
                DoubleSided = primitive.Material.DoubleSided
            };

            if (primitive.Material.FindChannel(KnownChannel.BaseColor.ToString()) is MaterialChannel baseColorChannel)
            {
                geometryNode.BaseColorFactor = baseColorChannel.Color;

                if (baseColorChannel.Texture != null)
                {
                    geometryNode.BaseColorTextureIndex = (uint)baseColorChannel.Texture.LogicalIndex;
                }
            }

            if (primitive.Material.FindChannel(KnownChannel.Normal.ToString()) is MaterialChannel normalChannel)
            {
                if (normalChannel.Texture != null)
                {
                    geometryNode.NormalTextureIndex = (uint)normalChannel.Texture.LogicalIndex;
                }
            }

            if (primitive.Material.FindChannel(KnownChannel.MetallicRoughness.ToString()) is MaterialChannel roughnessChannel)
            {
                if (roughnessChannel.Texture != null)
                {
                    geometryNode.RoughnessTextureIndex = (uint)roughnessChannel.Texture.LogicalIndex;
                }
            }

            uint vertexOffset = (uint)_vertices.Count;
            uint indexOffset = (uint)_indices.Count;

            AccelStructTriangles triangles = new()
            {
                VertexFormat = PixelFormat.R32G32B32Float,
                VertexStride = (uint)sizeof(Vertex),
                VertexCount = (uint)vertices.Count,
                VertexOffset = (uint)(sizeof(Vertex) * vertexOffset),
                IndexFormat = IndexFormat.U32,
                IndexCount = (uint)indices.Count,
                IndexOffset = sizeof(uint) * indexOffset,
                Transform = node.WorldMatrix
            };

            _vertices.AddRange(vertices);
            _indices.AddRange(indices.Select(index => index + vertexOffset));
            _nodes.Add(geometryNode);
            _triangles.Add(triangles);
        }
    }
}
