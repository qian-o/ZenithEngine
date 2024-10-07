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
using AlphaMode = SharpGLTF.Schema2.AlphaMode;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;

namespace Tests.RayTracing;

internal sealed unsafe class MainView : View
{
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
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 color, Vector4 tangent)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector2 TexCoord = texCoord;

        public Vector3 Color = color;

        public Vector4 Tangent = tangent;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GeometryNode
    {
        public uint VertexBuffer;

        public uint IndexBuffer;

        public bool AlphaMask;

        public float AlphaCutoff;

        public bool DoubleSided;

        public Vector4 BaseColorFactor;

        public uint BaseColorTextureIndex;

        public uint NormalTextureIndex;
    }

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;
    private readonly ViewController _viewController;
    private readonly CameraController _cameraController;

    private readonly List<Texture> _textures;
    private readonly List<TextureView> _textureViews;
    private readonly List<DeviceBuffer> _vertexBuffers;
    private readonly List<DeviceBuffer> _indexBuffers;
    private readonly List<GeometryNode> _geometryNodes;
    private readonly List<AccelerationStructureTriangles> _triangles;

    private readonly DeviceBuffer _cameraBuffer;
    private readonly DeviceBuffer _geometryNodesBuffer;
    private readonly BottomLevelAS _bottomLevel;
    private readonly TopLevelAS _topLevel;
    private readonly ResourceLayout _resourceLayout0;
    private readonly ResourceSet _resourceSet0;
    private readonly ResourceLayout _resourceLayout1;
    private readonly ResourceSet _resourceSet1;
    private readonly ResourceLayout _resourceLayout2;
    private readonly ResourceSet _resourceSet2;
    private readonly ResourceLayout _resourceLayout3;
    private readonly ResourceSet _resourceSet3;
    private readonly ResourceLayout _resourceLayout4;
    private readonly ResourceSet _resourceSet4;
    private readonly Pipeline _pipeline;
    private readonly CommandList _commandList;

    private Camera _camera;

    private Texture? _outputTexture;
    private TextureView? _outputTextureView;

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("Ray Tracing")
    {
        _device = device;
        _imGuiController = imGuiController;
        _viewController = new ViewController(this);
        _cameraController = new CameraController(_viewController);

        _textures = [];
        _textureViews = [];
        _vertexBuffers = [];
        _indexBuffers = [];
        _geometryNodes = [];
        _triangles = [];

        LoadGLTF("Assets/Models/Sponza/glTF/Sponza.gltf");

        _cameraBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<Camera>(1, BufferUsage.UniformBuffer));
        _geometryNodesBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<GeometryNode>(_geometryNodes.Count, BufferUsage.StorageBuffer));
        device.UpdateBuffer(_geometryNodesBuffer, _geometryNodes.ToArray());

        BottomLevelASDescription bottomLevelASDescription = new()
        {
            Geometries = [.. _triangles]
        };

        _bottomLevel = device.Factory.CreateBottomLevelAS(in bottomLevelASDescription);

        AccelerationStructureInstance accelerationStructureInstance = new()
        {
            Transform4x4 = Matrix4x4.Identity,
            InstanceID = 0,
            InstanceMask = 0xFF,
            InstanceContributionToHitGroupIndex = 0,
            Options = AccelerationStructureInstanceOptions.None,
            BottonLevel = _bottomLevel
        };

        TopLevelASDescription topLevelASDescription = new()
        {
            Instances = [accelerationStructureInstance],
            Options = AccelerationStructureOptions.PreferFastTrace
        };

        _topLevel = device.Factory.CreateTopLevelAS(in topLevelASDescription);

        _resourceLayout0 = device.Factory.CreateResourceLayout(new ResourceLayoutDescription(new ResourceLayoutElementDescription("as", ResourceKind.AccelerationStructure, ShaderStages.RayGeneration),
                                                                                             new ResourceLayoutElementDescription("camera", ResourceKind.UniformBuffer, ShaderStages.RayGeneration),
                                                                                             new ResourceLayoutElementDescription("geometryNodes", ResourceKind.StorageBuffer, ShaderStages.ClosestHit | ShaderStages.AnyHit),
                                                                                             new ResourceLayoutElementDescription("outputTexture", ResourceKind.StorageImage, ShaderStages.RayGeneration)));
        _resourceSet0 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout0, _topLevel, _cameraBuffer, _geometryNodesBuffer));

        _resourceLayout1 = device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_vertexBuffers.Count, new ResourceLayoutElementDescription("vertexArray", ResourceKind.StorageBuffer, ShaderStages.ClosestHit | ShaderStages.AnyHit)));
        _resourceSet1 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout1));
        _resourceSet1.UpdateBindless([.. _vertexBuffers]);

        _resourceLayout2 = device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_indexBuffers.Count, new ResourceLayoutElementDescription("indexArray", ResourceKind.StorageBuffer, ShaderStages.ClosestHit | ShaderStages.AnyHit)));
        _resourceSet2 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout2));
        _resourceSet2.UpdateBindless([.. _indexBuffers]);

        _resourceLayout3 = device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_textureViews.Count, new ResourceLayoutElementDescription("textureArray", ResourceKind.SampledImage, ShaderStages.ClosestHit | ShaderStages.AnyHit)));
        _resourceSet3 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout3));
        _resourceSet3.UpdateBindless([.. _textureViews]);

        _resourceLayout4 = device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless(2, new ResourceLayoutElementDescription("samplerArray", ResourceKind.Sampler, ShaderStages.ClosestHit | ShaderStages.AnyHit)));
        _resourceSet4 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout4));
        _resourceSet4.UpdateBindless([device.Aniso4xSampler, device.LinearSampler]);

        byte[] shaderBytes = Encoding.UTF8.GetBytes(File.ReadAllText("Assets/Shaders/rayTracing.hlsl"));

        ShaderDescription[] shaderDescriptions =
        [
            new ShaderDescription(ShaderStages.RayGeneration,  shaderBytes, "rayGen"),
            new ShaderDescription(ShaderStages.Miss,  shaderBytes, "miss"),
            new ShaderDescription(ShaderStages.ClosestHit, shaderBytes, "closestHit"),
            new ShaderDescription(ShaderStages.AnyHit, shaderBytes, "anyHit")
        ];

        Shader[] shaders = device.Factory.HlslToSpirv(shaderDescriptions);

        RaytracingPipelineDescription raytracingPipelineDescription = new()
        {
            Shaders = new RaytracingShaderDescription()
            {
                RayGenerationShader = shaders[0],
                MissShader = [shaders[1]],
                HitGroupShader =
                [
                    new HitGroupDescription()
                    {
                        ClosestHitShader = shaders[2],
                        AnyHitShader = shaders[3]
                    }
                ]
            },
            ResourceLayouts = [_resourceLayout0, _resourceLayout1, _resourceLayout2, _resourceLayout3, _resourceLayout4],
            MaxTraceRecursionDepth = 6
        };

        _pipeline = device.Factory.CreateRaytracingPipeline(raytracingPipelineDescription);
        _commandList = device.Factory.CreateGraphicsCommandList();

        foreach (Shader shader in shaders)
        {
            shader.Dispose();
        }

        _camera = new Camera()
        {
            Position = _cameraController.Position,
            Forward = _cameraController.Forward,
            Right = _cameraController.Right,
            Up = _cameraController.Up,
            NearPlane = _cameraController.NearPlane,
            FarPlane = _cameraController.FarPlane,
            Fov = _cameraController.FovRadians
        };
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
        _viewController.Update();
        _cameraController.Update(e.DeltaTime);

        if (ImGui.Begin("Properties"))
        {
            _cameraController.ShowEditor();

            ImGui.End();
        }

        _camera.Position = _cameraController.Position;
        _camera.Forward = _cameraController.Forward;
        _camera.Right = _cameraController.Right;
        _camera.Up = _cameraController.Up;
        _camera.NearPlane = _cameraController.NearPlane;
        _camera.FarPlane = _cameraController.FarPlane;
        _camera.Fov = _cameraController.FovRadians;

        _device.UpdateBuffer(_cameraBuffer, in _camera);
    }

    protected override void OnRender(RenderEventArgs e)
    {
        if (_outputTexture != null)
        {
            _commandList.Begin();

            _commandList.SetPipeline(_pipeline);

            _commandList.SetResourceSet(0, _resourceSet0);
            _commandList.SetResourceSet(1, _resourceSet1);
            _commandList.SetResourceSet(2, _resourceSet2);
            _commandList.SetResourceSet(3, _resourceSet3);
            _commandList.SetResourceSet(4, _resourceSet4);

            _commandList.DispatchRays(_outputTexture.Width, _outputTexture.Height, 1);

            _commandList.End();

            _device.SubmitCommands(_commandList);

            ImGui.Image(_imGuiController.GetBinding(_device.Factory, _outputTexture), new Vector2(_outputTexture.Width, _outputTexture.Height));
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        _outputTextureView?.Dispose();

        if (_outputTexture != null)
        {
            _imGuiController.RemoveBinding(_imGuiController.GetBinding(_device.Factory, _outputTexture));

            _outputTexture.Dispose();
        }

        _outputTexture = _device.Factory.CreateTexture(TextureDescription.Texture2D(e.Width,
                                                                                    e.Height,
                                                                                    1,
                                                                                    PixelFormat.R8G8B8A8UNorm,
                                                                                    TextureUsage.Storage | TextureUsage.Sampled));

        _outputTextureView = _device.Factory.CreateTextureView(_outputTexture);

        _resourceSet0.UpdateSet(_outputTextureView, 3);
    }

    protected override void Destroy()
    {
        _outputTextureView?.Dispose();
        _outputTexture?.Dispose();
    }

    private void LoadGLTF(string path)
    {
        ModelRoot root = ModelRoot.Load(path, new ReadSettings() { Validation = ValidationMode.Skip });

        #region Load Textures
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

                vertices.Add(new Vertex(position,
                                        normal,
                                        texCoord,
                                        color,
                                        tangent));
            }

            if (primitive.IndexAccessor != null)
            {
                indices.AddRange(primitive.IndexAccessor.AsIndicesArray());
            }

            const BufferUsage bufferUsage = BufferUsage.StorageBuffer | BufferUsage.AccelerationStructure;

            DeviceBuffer vertexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(vertices.Count, bufferUsage));
            _device.UpdateBuffer(vertexBuffer, vertices.ToArray());

            DeviceBuffer indexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(indices.Count, bufferUsage));
            _device.UpdateBuffer(indexBuffer, indices.ToArray());

            GeometryNode geometryNode = new()
            {
                VertexBuffer = (uint)_vertexBuffers.Count,
                IndexBuffer = (uint)_indexBuffers.Count,
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

            AccelerationStructureTriangles accelerationStructureTriangles = new()
            {
                VertexBuffer = vertexBuffer,
                VertexFormat = PixelFormat.R32G32B32Float,
                VertexStride = (uint)sizeof(Vertex),
                VertexCount = (uint)vertices.Count,
                VertexOffset = 0,
                IndexBuffer = indexBuffer,
                IndexFormat = IndexFormat.U32,
                IndexCount = (uint)indices.Count,
                IndexOffset = 0,
                Transform = node.WorldMatrix
            };

            _vertexBuffers.Add(vertexBuffer);
            _indexBuffers.Add(indexBuffer);
            _geometryNodes.Add(geometryNode);
            _triangles.Add(accelerationStructureTriangles);
        }
    }
}
