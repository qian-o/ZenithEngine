using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using StbImageSharp;
using Tests.AndroidApp.Helpers;
using Tests.Core.Helpers;
using AlphaMode = SharpGLTF.Schema2.AlphaMode;
using CameraController = Tests.AndroidApp.Controls.CameraController;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;

namespace Tests.AndroidApp.Samples;

internal unsafe class RayQuery : BaseSample
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

        public Matrix4x4 InvViewMatrix;

        public Matrix4x4 InvProjectionMatrix;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Light
    {
        public Vector3 Position;

        public Vector4 AmbientColor;

        public Vector4 DiffuseColor;
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct Param
    {
        public uint Width;

        public uint Height;

        public PixelOffsets PixelOffsets;

        public uint Samples;
    }

    [InlineArray(127)]
    private struct PixelOffsets
    {
        private Vector2 _element0;
    }
    #endregion

    private static readonly Vector2[] HaltonSequence =
    [
        new Vector2(5.000000e-01f, 6.666667e-01f),
        new Vector2(2.500000e-01f, 3.333333e-01f),
        new Vector2(7.500000e-01f, 2.222222e-01f),
        new Vector2(1.250000e-01f, 8.888889e-01f),
        new Vector2(6.250000e-01f, 5.555556e-01f),
        new Vector2(3.750000e-01f, 1.111111e-01f),
        new Vector2(8.750000e-01f, 7.777778e-01f),
        new Vector2(6.250000e-02f, 4.444444e-01f),
        new Vector2(5.625000e-01f, 7.407407e-02f),
        new Vector2(3.125000e-01f, 7.407407e-01f),
        new Vector2(8.125000e-01f, 4.074074e-01f),
        new Vector2(1.875000e-01f, 2.962963e-01f),
        new Vector2(6.875000e-01f, 9.629630e-01f),
        new Vector2(4.375000e-01f, 6.296296e-01f),
        new Vector2(9.375000e-01f, 1.851852e-01f),
        new Vector2(3.125000e-02f, 8.518519e-01f),
        new Vector2(5.312500e-01f, 5.185185e-01f),
        new Vector2(2.812500e-01f, 3.703704e-02f),
        new Vector2(7.812500e-01f, 7.037037e-01f),
        new Vector2(1.562500e-01f, 3.703704e-01f),
        new Vector2(6.562500e-01f, 2.592593e-01f),
        new Vector2(4.062500e-01f, 9.259259e-01f),
        new Vector2(9.062500e-01f, 5.925926e-01f),
        new Vector2(9.375000e-02f, 1.481481e-01f),
        new Vector2(5.937500e-01f, 8.148148e-01f),
        new Vector2(3.437500e-01f, 4.814815e-01f),
        new Vector2(8.437500e-01f, 2.469136e-02f),
        new Vector2(2.187500e-01f, 6.913580e-01f),
        new Vector2(7.187500e-01f, 3.580247e-01f),
        new Vector2(4.687500e-01f, 2.469136e-01f),
        new Vector2(9.687500e-01f, 9.135802e-01f),
        new Vector2(1.562500e-02f, 5.802469e-01f),
        new Vector2(5.156250e-01f, 1.358025e-01f),
        new Vector2(2.656250e-01f, 8.024691e-01f),
        new Vector2(7.656250e-01f, 4.691358e-01f),
        new Vector2(1.406250e-01f, 9.876543e-02f),
        new Vector2(6.406250e-01f, 7.654321e-01f),
        new Vector2(3.906250e-01f, 4.320988e-01f),
        new Vector2(8.906250e-01f, 3.209877e-01f),
        new Vector2(7.812500e-02f, 9.876543e-01f),
        new Vector2(5.781250e-01f, 6.543210e-01f),
        new Vector2(3.281250e-01f, 2.098765e-01f),
        new Vector2(8.281250e-01f, 8.765432e-01f),
        new Vector2(2.031250e-01f, 5.432099e-01f),
        new Vector2(7.031250e-01f, 6.172840e-02f),
        new Vector2(4.531250e-01f, 7.283951e-01f),
        new Vector2(9.531250e-01f, 3.950617e-01f),
        new Vector2(4.687500e-02f, 2.839506e-01f),
        new Vector2(5.468750e-01f, 9.506173e-01f),
        new Vector2(2.968750e-01f, 6.172840e-01f),
        new Vector2(7.968750e-01f, 1.728395e-01f),
        new Vector2(1.718750e-01f, 8.395062e-01f),
        new Vector2(6.718750e-01f, 5.061728e-01f),
        new Vector2(4.218750e-01f, 1.234568e-02f),
        new Vector2(9.218750e-01f, 6.790123e-01f),
        new Vector2(1.093750e-01f, 3.456790e-01f),
        new Vector2(6.093750e-01f, 2.345679e-01f),
        new Vector2(3.593750e-01f, 9.012346e-01f),
        new Vector2(8.593750e-01f, 5.679012e-01f),
        new Vector2(2.343750e-01f, 1.234568e-01f),
        new Vector2(7.343750e-01f, 7.901235e-01f),
        new Vector2(4.843750e-01f, 4.567901e-01f),
        new Vector2(9.843750e-01f, 8.641975e-02f),
        new Vector2(7.812500e-03f, 7.530864e-01f),
        new Vector2(5.078125e-01f, 4.197531e-01f),
        new Vector2(2.578125e-01f, 3.086420e-01f),
        new Vector2(7.578125e-01f, 9.753086e-01f),
        new Vector2(1.328125e-01f, 6.419753e-01f),
        new Vector2(6.328125e-01f, 1.975309e-01f),
        new Vector2(3.828125e-01f, 8.641975e-01f),
        new Vector2(8.828125e-01f, 5.308642e-01f),
        new Vector2(7.031250e-02f, 4.938272e-02f),
        new Vector2(5.703125e-01f, 7.160494e-01f),
        new Vector2(3.203125e-01f, 3.827160e-01f),
        new Vector2(8.203125e-01f, 2.716049e-01f),
        new Vector2(1.953125e-01f, 9.382716e-01f),
        new Vector2(6.953125e-01f, 6.049383e-01f),
        new Vector2(4.453125e-01f, 1.604938e-01f),
        new Vector2(9.453125e-01f, 8.271605e-01f),
        new Vector2(3.906250e-02f, 4.938272e-01f),
        new Vector2(5.390625e-01f, 8.230453e-03f),
        new Vector2(2.890625e-01f, 6.748971e-01f),
        new Vector2(7.890625e-01f, 3.415638e-01f),
        new Vector2(1.640625e-01f, 2.304527e-01f),
        new Vector2(6.640625e-01f, 8.971193e-01f),
        new Vector2(4.140625e-01f, 5.637860e-01f),
        new Vector2(9.140625e-01f, 1.193416e-01f),
        new Vector2(1.015625e-01f, 7.860082e-01f),
        new Vector2(6.015625e-01f, 4.526749e-01f),
        new Vector2(3.515625e-01f, 8.230453e-02f),
        new Vector2(8.515625e-01f, 7.489712e-01f),
        new Vector2(2.265625e-01f, 4.156379e-01f),
        new Vector2(7.265625e-01f, 3.045267e-01f),
        new Vector2(4.765625e-01f, 9.711934e-01f),
        new Vector2(9.765625e-01f, 6.378601e-01f),
        new Vector2(2.343750e-02f, 1.934156e-01f),
        new Vector2(5.234375e-01f, 8.600823e-01f),
        new Vector2(2.734375e-01f, 5.267490e-01f),
        new Vector2(7.734375e-01f, 4.526749e-02f),
        new Vector2(1.484375e-01f, 7.119342e-01f),
        new Vector2(6.484375e-01f, 3.786008e-01f),
        new Vector2(3.984375e-01f, 2.674897e-01f),
        new Vector2(8.984375e-01f, 9.341564e-01f),
        new Vector2(8.593750e-02f, 6.008230e-01f),
        new Vector2(5.859375e-01f, 1.563786e-01f),
        new Vector2(3.359375e-01f, 8.230453e-01f),
        new Vector2(8.359375e-01f, 4.897119e-01f),
        new Vector2(2.109375e-01f, 3.292181e-02f),
        new Vector2(7.109375e-01f, 6.995885e-01f),
        new Vector2(4.609375e-01f, 3.662551e-01f),
        new Vector2(9.609375e-01f, 2.551440e-01f),
        new Vector2(5.468750e-02f, 9.218107e-01f),
        new Vector2(5.546875e-01f, 5.884774e-01f),
        new Vector2(3.046875e-01f, 1.440329e-01f),
        new Vector2(8.046875e-01f, 8.106996e-01f),
        new Vector2(1.796875e-01f, 4.773663e-01f),
        new Vector2(6.796875e-01f, 1.069959e-01f),
        new Vector2(4.296875e-01f, 7.736626e-01f),
        new Vector2(9.296875e-01f, 4.403292e-01f),
        new Vector2(1.171875e-01f, 3.292181e-01f),
        new Vector2(6.171875e-01f, 9.958848e-01f),
        new Vector2(3.671875e-01f, 6.625514e-01f),
        new Vector2(8.671875e-01f, 2.181070e-01f),
        new Vector2(2.421875e-01f, 8.847737e-01f),
        new Vector2(7.421875e-01f, 5.514403e-01f),
        new Vector2(4.921875e-01f, 6.995885e-02f),
        new Vector2(9.921875e-01f, 7.366255e-01f),
    ];

    private readonly List<Texture> _textures = [];
    private readonly List<TextureView> _textureViews = [];
    private readonly List<Vertex> _vertices = [];
    private readonly List<uint> _indices = [];
    private readonly List<GeometryNode> _nodes = [];
    private readonly List<AccelStructTriangles> _triangles = [];
    private readonly List<Light> _lights = [];

    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _nodeBuffer = null!;
    private DeviceBuffer _cameraBuffer = null!;
    private DeviceBuffer _lightBuffer = null!;
    private DeviceBuffer _paramBuffer = null!;
    private BottomLevelAS _bottomLevel = null!;
    private TopLevelAS _topLevel = null!;
    private ResourceLayout _resourceLayout0 = null!;
    private ResourceSet _resourceSet0 = null!;
    private ResourceLayout _resourceLayout1 = null!;
    private ResourceSet _resourceSet1 = null!;
    private ResourceLayout _resourceLayout2 = null!;
    private ResourceSet _resourceSet2 = null!;
    private Pipeline? _pipeline = null!;

    private Param _param;

    public override void Load(Swapchain swapchain)
    {
        LoadGLTF("Assets/Models/Sponza/glTF", "Sponza.gltf");

        _lights.Add(new Light()
        {
            Position = new Vector3(-100.0f, 100.0f, 0.0f),
            AmbientColor = new Vector4(0.05f, 0.05f, 0.05f, 1.0f),
            DiffuseColor = new Vector4(1.0f, 0.9f, 0.7f, 1.0f)
        });

        _vertexBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(_vertices.Count, BufferUsage.VertexBuffer | BufferUsage.AccelerationStructure));
        App.Device.UpdateBuffer(_vertexBuffer, [.. _vertices]);

        _indexBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(_indices.Count, BufferUsage.IndexBuffer | BufferUsage.AccelerationStructure));
        App.Device.UpdateBuffer(_indexBuffer, [.. _indices]);

        _nodeBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<GeometryNode>(_nodes.Count, BufferUsage.StorageBuffer));
        App.Device.UpdateBuffer(_nodeBuffer, [.. _nodes]);

        _cameraBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Camera>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        _lightBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Light>(_lights.Count, BufferUsage.StorageBuffer));
        App.Device.UpdateBuffer(_lightBuffer, [.. _lights]);

        _paramBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Param>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        for (int i = 0; i < _triangles.Count; i++)
        {
            _triangles[i].VertexBuffer = _vertexBuffer;
            _triangles[i].IndexBuffer = _indexBuffer;
        }

        BottomLevelASDescription bottomLevelASDescription = new()
        {
            Geometries = [.. _triangles]
        };

        _bottomLevel = App.Device.Factory.CreateBottomLevelAS(in bottomLevelASDescription);

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

        _topLevel = App.Device.Factory.CreateTopLevelAS(in topLevelASDescription);

        ElementDescription[] elements0 =
        [
            new ElementDescription("as", ResourceKind.AccelerationStructure, ShaderStages.Pixel),
            new ElementDescription("geometryNodes", ResourceKind.StorageBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("camera", ResourceKind.ConstantBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("lights", ResourceKind.StorageBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("param", ResourceKind.ConstantBuffer, ShaderStages.Pixel)
        ];

        _resourceLayout0 = App.Device.Factory.CreateResourceLayout(new ResourceLayoutDescription(elements0));
        _resourceSet0 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout0,
                                                                                    _topLevel,
                                                                                    _nodeBuffer,
                                                                                    _cameraBuffer,
                                                                                    _lightBuffer,
                                                                                    _paramBuffer));

        ElementDescription[] elements1 =
        [
            new ElementDescription("textureArray", ResourceKind.SampledImage, ShaderStages.Pixel)
        ];

        _resourceLayout1 = App.Device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_textureViews.Count, elements1));
        _resourceSet1 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout1));
        _resourceSet1.UpdateBindless([.. _textureViews]);

        ElementDescription[] elements2 =
        [
            new ElementDescription("samplerArray", ResourceKind.Sampler, ShaderStages.Pixel)
        ];

        _resourceLayout2 = App.Device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless(2, elements2));
        _resourceSet2 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout2));
        _resourceSet2.UpdateBindless([App.Device.Aniso4xSampler, App.Device.LinearSampler]);

        VertexElementDescription positionElement = new(nameof(Vertex.Position), VertexElementFormat.Float3);
        VertexElementDescription normalElement = new(nameof(Vertex.Normal), VertexElementFormat.Float3);
        VertexElementDescription texCoordElement = new(nameof(Vertex.TexCoord), VertexElementFormat.Float2);
        VertexElementDescription colorElement = new(nameof(Vertex.Color), VertexElementFormat.Float3);
        VertexElementDescription tangentElement = new(nameof(Vertex.Tangent), VertexElementFormat.Float4);
        VertexElementDescription nodeIndexElement = new(nameof(Vertex.NodeIndex), VertexElementFormat.UInt1);

        VertexLayoutDescription vertexLayout = new(positionElement, normalElement, texCoordElement, colorElement, tangentElement, nodeIndexElement);

        using Shader vs = App.Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, [.. new FileReader("Assets/Shaders").ReadFile("rayQuery.vs.hlsl.spv")], "main"));
        using Shader ps = App.Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Pixel, [.. new FileReader("Assets/Shaders").ReadFile("rayQuery.ps.hlsl.spv")], "main"));

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_resourceLayout0, _resourceLayout1, _resourceLayout2],
            Shaders = new GraphicsShaderDescription([vertexLayout], [vs, ps]),
            Outputs = swapchain.Framebuffer.OutputDescription
        };

        _pipeline = App.Device.Factory.CreateGraphicsPipeline(pipelineDescription);

        _param = new Param()
        {
            Samples = 4
        };

        for (int i = 0; i < HaltonSequence.Length; i++)
        {
            _param.PixelOffsets[i] = HaltonSequence[i];
        }
    }

    public override void Update(Swapchain swapchain, float width, float height, CameraController camera, float deltaTime, float totalTime)
    {
        base.Update(swapchain, width, height, camera, deltaTime, totalTime);

        Camera cameraBuffer = new()
        {
            Position = camera.Position,
            Forward = camera.Forward,
            Right = camera.Right,
            Up = camera.Up,
            NearPlane = camera.NearPlane,
            FarPlane = camera.FarPlane,
            Fov = camera.Fov.ToRadians(),
            ViewMatrix = camera.GetView(),
            ProjectionMatrix = camera.GetProjection(width, height)
        };
        Matrix4x4.Invert(cameraBuffer.ViewMatrix, out cameraBuffer.InvViewMatrix);
        Matrix4x4.Invert(cameraBuffer.ProjectionMatrix, out cameraBuffer.InvProjectionMatrix);

        App.Device.UpdateBuffer(_cameraBuffer, in cameraBuffer);

        _param.Width = (uint)width;
        _param.Height = (uint)height;

        App.Device.UpdateBuffer(_paramBuffer, in _param);
    }

    public override void Render(CommandList commandList, Swapchain swapchain, float deltaTime, float totalTime)
    {
        base.Render(commandList, swapchain, deltaTime, totalTime);

        commandList.SetFramebuffer(swapchain.Framebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.Black);
        commandList.ClearDepthStencil(1.0f);

        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);
        commandList.SetPipeline(_pipeline!);
        commandList.SetResourceSet(0, _resourceSet0);
        commandList.SetResourceSet(1, _resourceSet1);
        commandList.SetResourceSet(2, _resourceSet2);

        commandList.DrawIndexed((uint)_indices.Count, 1, 0, 0, 0);
    }

    public override void Unload()
    {
    }

    private void LoadGLTF(string path, string name)
    {
        ModelRoot root = ModelRoot.Load(name, ReadContext.Create(new FileReader(path).ReadFile));

        #region Load Textures
        foreach (GLTFTexture gltfTexture in root.LogicalTextures)
        {
            using Stream stream = gltfTexture.PrimaryImage.Content.Open();

            if (ImageInfo.FromStream(stream) is not ImageInfo imageInfo)
            {
                return;
            }

            int width = imageInfo.Width;
            int height = imageInfo.Height;

            uint mipLevels = Math.Max(1, (uint)MathF.Log2(Math.Max(width, height))) + 1;

            TextureDescription description = TextureDescription.Texture2D((uint)width, (uint)height, mipLevels, PixelFormat.R8G8B8A8UNorm, TextureUsage.Sampled | TextureUsage.GenerateMipmaps);

            Texture texture = App.Device.Factory.CreateTexture(in description);
            texture.Name = gltfTexture.Name;

            TextureView textureView = App.Device.Factory.CreateTextureView(texture);
            textureView.Name = gltfTexture.Name;

            _textures.Add(texture);
            _textureViews.Add(textureView);
        }
        #endregion

        #region Load Meshes
        foreach (Node node in root.LogicalNodes)
        {
            LoadNode(node);
        }
        #endregion

        AddBackgroundTask(LoadTextures, root, _textures);
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
                VertexStrideInBytes = (uint)sizeof(Vertex),
                VertexCount = (uint)vertices.Count,
                VertexOffsetInBytes = 0,
                IndexFormat = IndexFormat.U32,
                IndexCount = (uint)indices.Count,
                IndexOffsetInBytes = sizeof(uint) * indexOffset,
                Transform = node.WorldMatrix
            };

            _vertices.AddRange(vertices);
            _indices.AddRange(indices.Select(index => index + vertexOffset));
            _nodes.Add(geometryNode);
            _triangles.Add(triangles);
        }
    }

    private void LoadTextures(object[] args)
    {
        ModelRoot root = (ModelRoot)args[0];
        List<Texture> textures = (List<Texture>)args[1];

        Parallel.For(0, textures.Count, i =>
        {
            using Stream stream = root.LogicalTextures[i].PrimaryImage.Content.Open();

            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            root.LogicalTextures[i].ClearImages();

            AddRenderTask(WriteTexture, textures[i], image);
        });
    }

    private void WriteTexture(CommandList commandList, object[] args)
    {
        Texture texture = (Texture)args[0];
        ImageResult image = (ImageResult)args[1];

        commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)image.Width, (uint)image.Height, 1, 0, 0);
        commandList.GenerateMipmaps(texture);
    }
}
