﻿using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Graphics.Vulkan.ImGui;
using Hexa.NET.ImGui;
using Tests.Core;

namespace Tests.RayTracing;

internal sealed unsafe class MainView : View
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex(Vector3 position, Vector3 normal, Vector3 color)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector3 Color = color;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CameraProperties
    {
        public Matrix4x4 ViewInverse;

        public Matrix4x4 ProjInverse;
    }

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;

    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly DeviceBuffer _buffer;
    private readonly BottomLevelAS _bottomLevelAS;
    private readonly TopLevelAS _topLevelAS;
    private readonly ResourceLayout _resourceLayout;
    private readonly ResourceSet _resourceSet;
    private readonly Pipeline _pipeline;
    private readonly CommandList _commandList;

    private Texture? _outputTexture;
    private TextureView? _outputTextureView;

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("Ray Tracing")
    {
        Vertex[] vertices =
        [
            new(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 0.0f)),
            new(new Vector3(1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f)),
            new(new Vector3(1.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 1.0f))
        ];

        ushort[] indices = [0, 1, 2];

        string rayGen = File.ReadAllText("Assets/Shaders/rayGen.hlsl");
        string miss = File.ReadAllText("Assets/Shaders/miss.hlsl");
        string closestHit = File.ReadAllText("Assets/Shaders/closestHit.hlsl");

        _device = device;
        _imGuiController = imGuiController;

        _vertexBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(vertices.Length, BufferUsage.StorageBuffer | BufferUsage.AccelerationStructure));
        device.UpdateBuffer(_vertexBuffer, vertices);

        _indexBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<ushort>(indices.Length, BufferUsage.StorageBuffer | BufferUsage.AccelerationStructure));
        device.UpdateBuffer(_indexBuffer, indices);

        _buffer = device.Factory.CreateBuffer(BufferDescription.Buffer<CameraProperties>(1, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        AccelerationStructureTriangles accelerationStructureTriangles = new()
        {
            VertexBuffer = _vertexBuffer,
            VertexFormat = PixelFormat.R32G32B32Float,
            VertexStride = (uint)sizeof(Vertex),
            VertexCount = (uint)vertices.Length,
            VertexOffset = 0,
            IndexBuffer = _indexBuffer,
            IndexFormat = IndexFormat.U16,
            IndexCount = (uint)indices.Length,
            IndexOffset = 0,
        };

        _bottomLevelAS = device.Factory.CreateBottomLevelAS(new BottomLevelASDescription(accelerationStructureTriangles));

        AccelerationStructureInstance accelerationStructureInstance = new()
        {
            Transform4x4 = Matrix4x4.Identity,
            InstanceID = 0,
            InstanceMask = 0xFF,
            InstanceContributionToHitGroupIndex = 0,
            Options = AccelerationStructureInstanceOptions.None,
            BottonLevel = _bottomLevelAS
        };

        _topLevelAS = device.Factory.CreateTopLevelAS(new TopLevelASDescription(AccelerationStructureOptions.AllowUpdate, accelerationStructureInstance));

        _resourceLayout = device.Factory.CreateResourceLayout(new ResourceLayoutDescription(new ResourceLayoutElementDescription("rs", ResourceKind.AccelerationStructure, ShaderStages.RayGeneration),
                                                                                            new ResourceLayoutElementDescription("outputTexture", ResourceKind.StorageImage, ShaderStages.RayGeneration),
                                                                                            new ResourceLayoutElementDescription("cameraProperties", ResourceKind.UniformBuffer, ShaderStages.RayGeneration)));
        _resourceSet = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout, _topLevelAS));
        _resourceSet.UpdateSet(_buffer, 2);

        ShaderDescription[] shaderDescriptions =
        [
            new ShaderDescription(ShaderStages.RayGeneration,  Encoding.UTF8.GetBytes(rayGen), "rayGen"),
            new ShaderDescription(ShaderStages.Miss,  Encoding.UTF8.GetBytes(miss), "miss"),
            new ShaderDescription(ShaderStages.ClosestHit, Encoding.UTF8.GetBytes(closestHit), "closestHit")
        ];

        Shader[] shaders = device.Factory.HlslToSpirv(shaderDescriptions);

        RaytracingPipelineDescription raytracingPipelineDescription = new()
        {
            Shaders = new RaytracingShaderStateDescription()
            {
                RayGenerationShader = shaders[0],
                MissShader = [shaders[1]],
                ClosestHitShader = [shaders[2]]
            },
            HitGroups =
            [
                new HitGroupDescription()
                {
                    Name = "RaygenGroup",
                    Type = HitGroupType.General,
                    GeneralEntryPoint = "rayGen"
                },
                new HitGroupDescription()
                {
                    Name = "MissGroup",
                    Type = HitGroupType.General,
                    GeneralEntryPoint = "miss"
                },
                new HitGroupDescription()
                {
                    Name = "HitGroup",
                    Type = HitGroupType.Triangles,
                    ClosestHitEntryPoint = "closestHit"
                }
            ],
            ResourceLayouts = [_resourceLayout],
            MaxTraceRecursionDepth = 1,
            MaxPayloadSizeInBytes = 64,
            MaxAttributeSizeInBytes = 32
        };

        _pipeline = device.Factory.CreateRaytracingPipeline(raytracingPipelineDescription);
        _commandList = device.Factory.CreateGraphicsCommandList();
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
        Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, -2.5f), Vector3.Zero, Vector3.UnitY);
        Matrix4x4 perspective = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4.0f, (float)Width / Height, 0.1f, 512.0f);

        CameraProperties cameraProperties = new()
        {
            ViewInverse = Matrix4x4.Transpose(view),
            ProjInverse = Matrix4x4.Transpose(perspective)
        };

        _device.UpdateBuffer(_buffer, in cameraProperties);
    }

    protected override void OnRender(RenderEventArgs e)
    {
        if (_outputTexture != null)
        {
            _commandList.Begin();

            _commandList.SetPipeline(_pipeline);

            _commandList.SetResourceSet(0, _resourceSet);

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

        _resourceSet.UpdateSet(_outputTextureView, 1);
    }

    protected override void Destroy()
    {
    }
}
