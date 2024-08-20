using System.Numerics;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using Tests.Core;

namespace Tests.SDFFontTexture;

internal sealed unsafe class MainView : View
{
    #region Structs
    private struct Vertex
    {
        public Vector3 Position;

        public Vector2 TexCoord;
    }

    private struct UniformBufferObject
    {
        public Matrix4x4 Model;

        public Matrix4x4 View;

        public Matrix4x4 Projection;
    }

    private struct Properties
    {
        public float PxRange;
    }
    #endregion

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;
    private readonly FontController _fontController;

    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly DeviceBuffer _uniformBuffer;
    private readonly DeviceBuffer _normalBuffer;
    private readonly ResourceLayout _resourceLayout1;
    private readonly ResourceSet _resourceSet1;
    private readonly ResourceLayout _resourceLayout2;
    private readonly Dictionary<char, ResourceSet> _charResourceSets = [];
    private readonly Dictionary<Texture, TextureView> _charTextureViews = [];
    private readonly Shader[] _shaders;
    private readonly VertexLayoutDescription[] _vertexLayoutDescriptions;
    private readonly CommandList _commandList;

    private FramebufferObject? framebufferObject;
    private Pipeline? pipeline;

    private string chars = "A";
    private Vector3 position = Vector3.Zero;
    private Properties properties = new() { PxRange = 5.0f };

    public MainView(GraphicsDevice device, ImGuiController imGuiController)
    {
        Title = "SDF Font Texture";

        _device = device;
        _imGuiController = imGuiController;
        _fontController = new FontController(device, "Assets/Fonts/simhei.ttf", 0, 128);

        Vertex[] vertices =
        [
            new() { Position = new Vector3(-0.5f, -0.5f, 0.0f), TexCoord = new Vector2(0.0f, 1.0f) },
            new() { Position = new Vector3(0.5f, -0.5f, 0.0f), TexCoord = new Vector2(1.0f, 1.0f) },
            new() { Position = new Vector3(0.5f, 0.5f, 0.0f), TexCoord = new Vector2(1.0f, 0.0f) },
            new() { Position = new Vector3(-0.5f, 0.5f, 0.0f), TexCoord = new Vector2(0.0f, 0.0f) }
        ];

        uint[] indices = [0, 1, 2, 2, 3, 0];

        _vertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(Vertex) * vertices.Length), BufferUsage.VertexBuffer));
        device.UpdateBuffer(_vertexBuffer, 0, vertices);

        _indexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * indices.Length), BufferUsage.IndexBuffer));
        device.UpdateBuffer(_indexBuffer, 0, indices);

        _uniformBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(UniformBufferObject), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _normalBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(Properties), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        ResourceLayoutElementDescription uboDescription = new("ubo", ResourceKind.UniformBuffer, ShaderStages.Vertex);
        ResourceLayoutElementDescription normalDescription = new("properties", ResourceKind.UniformBuffer, ShaderStages.Fragment);
        ResourceLayoutElementDescription textureSamplerDescription = new("textureSampler", ResourceKind.Sampler, ShaderStages.Fragment);

        _resourceLayout1 = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(uboDescription, normalDescription, textureSamplerDescription));

        _resourceSet1 = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_resourceLayout1, _uniformBuffer, _normalBuffer, device.LinearSampler));

        ResourceLayoutElementDescription sdfDescription = new("textureSDF", ResourceKind.TextureReadOnly, ShaderStages.Fragment);

        _resourceLayout2 = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(sdfDescription));

        string hlsl = File.ReadAllText("Assets/Shaders/SDF.hlsl");
        _shaders = device.ResourceFactory.CreateFromSpirv(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(hlsl), "mainVS"),
                                                          new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(hlsl), "mainPS"));

        _vertexLayoutDescriptions =
        [
            new(new VertexElementDescription("Position", VertexElementFormat.Float3), new VertexElementDescription("TexCoord", VertexElementFormat.Float2))
        ];

        _commandList = device.ResourceFactory.CreateGraphicsCommandList();
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
    }

    protected override void OnRender(RenderEventArgs e)
    {
        if (ImGui.Begin("Font Texture"))
        {
            ImGui.InputText("SDF Character", ref chars, 10);
            ImGui.DragFloat3("Position", ref position, 0.1f);

            chars = chars.Trim();

            ImGui.End();
        }

        if (!string.IsNullOrEmpty(chars))
        {
            char c = chars[0];

            if (!_charResourceSets.TryGetValue(c, out ResourceSet? resourceSet2))
            {
                Texture texture = _fontController.GetTexture(c);

                TextureView textureView = _device.ResourceFactory.CreateTextureView(texture);

                ResourceSetDescription resourceSetDescription = new(_resourceLayout2, textureView);

                resourceSet2 = _device.ResourceFactory.CreateResourceSet(resourceSetDescription);

                _charResourceSets.Add(c, resourceSet2);
                _charTextureViews.Add(texture, textureView);
            }

            if (framebufferObject != null && pipeline != null)
            {
                Character character = _fontController.GetCharacter(c);

                UniformBufferObject ubo = new()
                {
                    Model = Matrix4x4.CreateScale(new Vector3((float)character.Width / character.Height, 1.0f, 1.0f)) * Matrix4x4.CreateTranslation(position),
                    View = Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 2.0f), Vector3.Zero, Vector3.UnitY),
                    Projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)framebufferObject.Width / framebufferObject.Height, 0.1f, 1000.0f)
                };

                properties.PxRange = Math.Max(framebufferObject.Width, framebufferObject.Height);

                _commandList.Begin();

                _commandList.UpdateBuffer(_uniformBuffer, 0, ref ubo);
                _commandList.UpdateBuffer(_normalBuffer, 0, ref properties);

                _commandList.SetFramebuffer(framebufferObject.Framebuffer);
                _commandList.ClearColorTarget(0, RgbaFloat.Black);
                _commandList.ClearDepthStencil(1.0f);

                _commandList.SetVertexBuffer(0, _vertexBuffer);
                _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);
                _commandList.SetPipeline(pipeline);
                _commandList.SetGraphicsResourceSet(0, _resourceSet1);
                _commandList.SetGraphicsResourceSet(1, resourceSet2);

                _commandList.DrawIndexed(_indexBuffer.SizeInBytes / sizeof(uint), 1, 0, 0, 0);

                framebufferObject.Present(_commandList);

                _commandList.End();

                _device.SubmitCommands(_commandList);

                ImGui.Image(_imGuiController.GetOrCreateImGuiBinding(_device.ResourceFactory, framebufferObject.PresentTexture), new Vector2(framebufferObject.Width, framebufferObject.Height));
            }
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (framebufferObject != null)
        {
            _imGuiController.RemoveImGuiBinding(_imGuiController.GetOrCreateImGuiBinding(_device.ResourceFactory, framebufferObject.PresentTexture));

            framebufferObject.Dispose();
        }
        framebufferObject = new FramebufferObject(_device, (int)e.Width, (int)e.Height);

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_resourceLayout1, _resourceLayout2],
            ShaderSet = new ShaderSetDescription(_vertexLayoutDescriptions, _shaders),
            Outputs = framebufferObject.Framebuffer.OutputDescription
        };

        pipeline?.Dispose();
        pipeline = _device.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
    }

    protected override void Destroy()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _uniformBuffer.Dispose();
        _normalBuffer.Dispose();
        _resourceLayout1.Dispose();
        _resourceSet1.Dispose();
        _resourceLayout2.Dispose();

        foreach (TextureView textureView in _charTextureViews.Values)
        {
            textureView.Dispose();
        }

        foreach (ResourceSet resourceSet in _charResourceSets.Values)
        {
            resourceSet.Dispose();
        }

        foreach (Shader shader in _shaders)
        {
            shader.Dispose();
        }

        _commandList.Dispose();

        framebufferObject?.Dispose();
        pipeline?.Dispose();

        _fontController.Dispose();
    }
}
