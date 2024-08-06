using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Enums;
using Renderer.Models;
using Renderer.Structs;
using Scene = Renderer.Components.Scene;

namespace Renderer.Scenes;

internal sealed unsafe class GLTFAnimationScene : Scene
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
    #endregion

    private GLTF _busterDrone = null!;
    private DeviceBuffer _uboBuffer = null!;
    private ResourceLayout _uboLayout = null!;
    private ResourceSet _uboSet = null!;
    private ResourceLayout _materialLayout = null!;
    private ResourceSet[] _materialSets = null!;
    private Shader[] _shaders = null!;
    private VertexLayoutDescription[] _vertexLayoutDescriptions = null!;

    private Pipeline[]? _pipelines;

    protected override void Initialize()
    {
        Title = "GLTF Animation Scene";

        string hlsl = File.ReadAllText("Assets/Shaders/GLTF.hlsl");

        _busterDrone = GLTF.Load("Assets/Models/buster_drone/scene.gltf");
        _uboBuffer = App.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(UBO), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        ResourceLayoutDescription uboLayoutDescription = new(new ResourceLayoutElementDescription("UBO", ResourceKind.UniformBuffer, ShaderStages.Vertex));
        ResourceLayoutDescription materialLayoutDescription = new(new ResourceLayoutElementDescription("textureColorMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("textureSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("textureNormalMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("normalSampler", ResourceKind.Sampler, ShaderStages.Fragment));

        _uboLayout = App.ResourceFactory.CreateResourceLayout(in uboLayoutDescription);
        _uboSet = App.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_uboLayout, _uboBuffer));

        _materialLayout = App.ResourceFactory.CreateResourceLayout(in materialLayoutDescription);
        _materialSets = new ResourceSet[_busterDrone.Materials.Length];
        for (int i = 0; i < _busterDrone.Materials.Length; i++)
        {
            ResourceSetDescription materialSetDescription = new(_materialLayout,
                                                                _busterDrone.TextureViews[(int)_busterDrone.Materials[i].BaseColorTextureIndex],
                                                                App.GraphicsDevice.LinearSampler,
                                                                _busterDrone.TextureViews[(int)_busterDrone.Materials[i].NormalTextureIndex],
                                                                App.GraphicsDevice.LinearSampler);

            _materialSets[i] = App.ResourceFactory.CreateResourceSet(in materialSetDescription);
        }

        VertexElementDescription positionDescription = new("Position", VertexElementFormat.Float3);
        VertexElementDescription normalDescription = new("Normal", VertexElementFormat.Float3);
        VertexElementDescription texCoordDescription = new("TexCoord", VertexElementFormat.Float2);
        VertexElementDescription colorDescription = new("Color", VertexElementFormat.Float3);
        VertexElementDescription tangentDescription = new("Tangent", VertexElementFormat.Float4);

        _shaders = App.ResourceFactory.CreateFromSpirv(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(hlsl), "mainVS"),
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

        _pipelines = new Pipeline[_busterDrone.Materials.Length];
        for (int i = 0; i < _busterDrone.Materials.Length; i++)
        {
            bool alphaMask = _busterDrone.Materials[i].AlphaMode == AlphaMode.Mask;
            float alphaCutoff = _busterDrone.Materials[i].AlphaCutoff;

            GraphicsPipelineDescription pipelineDescription = new()
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = _busterDrone.Materials[i].DoubleSided ? RasterizerStateDescription.CullNone : RasterizerStateDescription.Default,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = [_uboLayout, _materialLayout],
                ShaderSet = new ShaderSetDescription(_vertexLayoutDescriptions, _shaders, [new SpecializationConstant(0, alphaMask), new SpecializationConstant(1, alphaCutoff)]),
                Outputs = framebuffer.OutputDescription
            };

            _pipelines[i] = App.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
        }
    }

    protected override void UpdateCore(UpdateEventArgs e)
    {
        UBO ubo = new()
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, Width / (float)Height, 0.1f, 1000.0f),
            View = Matrix4x4.CreateLookAt(new Vector3(7.8f, 2.1f, 0.0f), Vector3.Zero, Vector3.UnitY),
            Model = _busterDrone.Nodes[0].LocalTransform,
            LightPos = Vector4.Transform(new Vector4(0.0f, 2.5f, 0.0f, 1.0f), Matrix4x4.CreateRotationX(MathF.Sin(e.TotalTime))),
            ViewPos = new Vector4(new Vector3(7.8f, 2.1f, 0.0f), 1.0f)
        };

        App.GraphicsDevice.UpdateBuffer(_uboBuffer, 0, [ubo]);
    }

    protected override void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e)
    {
        if (_pipelines == null)
        {
            return;
        }

        commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
        commandList.ClearDepthStencil(1.0f);

        commandList.SetVertexBuffer(0, _busterDrone.VertexBuffer);
        commandList.SetIndexBuffer(_busterDrone.IndexBuffer, IndexFormat.U32);

        foreach (Node node in _busterDrone.Nodes)
        {
            DrawNode(commandList, node);
        }
    }

    protected override void Destroy()
    {
        if (_pipelines != null)
        {
            foreach (Pipeline pipeline in _pipelines)
            {
                pipeline.Dispose();
            }
        }

        foreach (Shader shader in _shaders)
        {
            shader.Dispose();
        }

        foreach (ResourceSet resourceSet in _materialSets)
        {
            resourceSet.Dispose();
        }

        _materialLayout.Dispose();

        _uboSet.Dispose();
        _uboLayout.Dispose();
        _uboBuffer.Dispose();
        _busterDrone.Dispose();

        base.Destroy();
    }

    private void DrawNode(CommandList commandList, Node node)
    {
        foreach (Primitive primitive in node.Primitives)
        {
            commandList.SetPipeline(_pipelines![primitive.MaterialIndex]);
            commandList.SetGraphicsResourceSet(0, _uboSet);
            commandList.SetGraphicsResourceSet(1, _materialSets[primitive.MaterialIndex]);
            commandList.DrawIndexed(primitive.IndexCount, 1, primitive.FirstIndex, 0, 0);
        }

        foreach (Node children in node.Children)
        {
            DrawNode(commandList, children);
        }
    }
}
