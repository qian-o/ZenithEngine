using System.Numerics;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using StbImageSharp;
using Tests.Core;
using Tests.SDFFontTexture.Models;

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
    private readonly MultiAtlasGenerator _multiAtlasGenerator;

    private readonly Layout _layout;
    private readonly Texture _sdfTexture;
    private readonly TextureView _sdfTextureView;
    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly DeviceBuffer _uniformBuffer;
    private readonly DeviceBuffer _normalBuffer;
    private readonly ResourceLayout _resourceLayout;
    private readonly ResourceSet _resourceSet;
    private readonly Shader[] _shaders;
    private readonly VertexLayoutDescription[] _vertexLayoutDescriptions;
    private readonly CommandList _commandList;

    private FramebufferObject? framebufferObject;
    private Pipeline? pipeline;

    private string str = "ABC";
    private Vector3 position = Vector3.Zero;
    private Properties properties = new() { PxRange = 5.0f };

    public MainView(GraphicsDevice device, ImGuiController imGuiController)
    {
        Title = "SDF Font Texture";

        _device = device;
        _imGuiController = imGuiController;
        _multiAtlasGenerator = new("Assets/Fonts/SIMYOU.TTF")
        {
            AtlasType = AtlasType.MTSDF,
            EmSize = 64,
            Padding = 2
        };

        _layout = _multiAtlasGenerator.Generate();

        byte[] bytes = File.ReadAllBytes(_layout.PngPath!);

        ImageResult imageResult = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

        _sdfTexture = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)imageResult.Width,
                                                                                        (uint)imageResult.Height,
                                                                                        1,
                                                                                        PixelFormat.R8G8B8A8UNorm,
                                                                                        TextureUsage.Sampled));

        device.UpdateTexture(_sdfTexture,
                             imageResult.Data,
                             0,
                             0,
                             0,
                             (uint)imageResult.Width,
                             (uint)imageResult.Height,
                             1,
                             0,
                             0);

        _sdfTextureView = device.ResourceFactory.CreateTextureView(new TextureViewDescription(_sdfTexture));

        Vertex[] vertices =
        [
            new() { Position = new Vector3(-0.5f, -0.5f, 0.0f), TexCoord = new Vector2(0.0f, 1.0f) },
            new() { Position = new Vector3(0.5f, -0.5f, 0.0f), TexCoord = new Vector2(1.0f, 1.0f) },
            new() { Position = new Vector3(0.5f, 0.5f, 0.0f), TexCoord = new Vector2(1.0f, 0.0f) },
            new() { Position = new Vector3(-0.5f, 0.5f, 0.0f), TexCoord = new Vector2(0.0f, 0.0f) }
        ];

        uint[] indices = [0, 1, 2, 2, 3, 0];

        _vertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(Vertex) * vertices.Length), BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        device.UpdateBuffer(_vertexBuffer, 0, vertices);

        _indexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * indices.Length), BufferUsage.IndexBuffer));
        device.UpdateBuffer(_indexBuffer, 0, indices);

        _uniformBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(UniformBufferObject), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _normalBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(Properties), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        ResourceLayoutElementDescription uboDescription = new("ubo", ResourceKind.UniformBuffer, ShaderStages.Vertex);
        ResourceLayoutElementDescription normalDescription = new("properties", ResourceKind.UniformBuffer, ShaderStages.Fragment);
        ResourceLayoutElementDescription msdfDescription = new("msdf", ResourceKind.TextureReadOnly, ShaderStages.Fragment);
        ResourceLayoutElementDescription msdfSamplerDescription = new("msdfSampler", ResourceKind.Sampler, ShaderStages.Fragment);

        _resourceLayout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(uboDescription, normalDescription, msdfDescription, msdfSamplerDescription));

        _resourceSet = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_resourceLayout, _uniformBuffer, _normalBuffer, _sdfTextureView, _device.LinearSampler));

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
        if (ImGui.Begin("Settings"))
        {
            ImGui.InputText("Char", ref str, 20);
            ImGui.DragFloat3("Position", ref position, 0.1f);

            ImGui.End();
        }

        if (framebufferObject != null)
        {
            _commandList.Begin();

            _commandList.SetFramebuffer(framebufferObject.Framebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1.0f);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);
            _commandList.SetPipeline(pipeline!);
            _commandList.SetGraphicsResourceSet(0, _resourceSet);

            float offset = 0.0f;
            foreach (char @char in str)
            {
                Glyph? glyph = _layout.Glyphs!.FirstOrDefault(x => x.UniCode == @char);
                glyph ??= _layout.Glyphs!.First(x => x.UniCode == '?');

                if (glyph.PlaneBounds.Width > 0)
                {
                    float vertex1 = glyph.PlaneBounds.Left;
                    float vertex2 = glyph.PlaneBounds.Right;
                    float vertex3 = glyph.PlaneBounds.Top;
                    float vertex4 = glyph.PlaneBounds.Bottom;

                    float beginU = (glyph.AtlasBounds.Left + _multiAtlasGenerator.Padding) / _layout.Atlas!.Width;
                    float endU = (glyph.AtlasBounds.Right - _multiAtlasGenerator.Padding) / _layout.Atlas!.Width;
                    float beginV = (_layout.Atlas!.Height - glyph.AtlasBounds.Top + _multiAtlasGenerator.Padding) / _layout.Atlas!.Height;
                    float endV = (_layout.Atlas!.Height - glyph.AtlasBounds.Bottom - _multiAtlasGenerator.Padding) / _layout.Atlas!.Height;

                    Vertex[] vertices =
                    [
                        new() { Position = new Vector3(vertex1, vertex4, 0.0f), TexCoord = new Vector2(beginU, endV) },
                        new() { Position = new Vector3(vertex2, vertex4, 0.0f), TexCoord = new Vector2(endU, endV) },
                        new() { Position = new Vector3(vertex2, vertex3, 0.0f), TexCoord = new Vector2(endU, beginV) },
                        new() { Position = new Vector3(vertex1, vertex3, 0.0f), TexCoord = new Vector2(beginU, beginV) }
                    ];

                    Matrix4x4 translation = Matrix4x4.CreateTranslation(new Vector3(offset, 0.0f, 0.0f));
                    Matrix4x4 model = translation;

                    UniformBufferObject ubo = new()
                    {
                        Model = model * Matrix4x4.CreateTranslation(position),
                        View = Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 2.0f), Vector3.Zero, Vector3.UnitY),
                        Projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)framebufferObject.Width / framebufferObject.Height, 0.1f, 1000.0f)
                    };

                    properties.PxRange = _layout.Atlas!.DistanceRange;

                    _commandList.UpdateBuffer(_vertexBuffer, 0, vertices);
                    _commandList.UpdateBuffer(_uniformBuffer, 0, ref ubo);
                    _commandList.UpdateBuffer(_normalBuffer, 0, ref properties);

                    _commandList.DrawIndexed(_indexBuffer.SizeInBytes / sizeof(uint), 1, 0, 0, 0);
                }

                offset += glyph.Advance;
            }

            framebufferObject.Present(_commandList);

            _commandList.End();

            _device.SubmitCommands(_commandList);

            ImGui.Image(_imGuiController.GetOrCreateImGuiBinding(_device.ResourceFactory, framebufferObject.PresentTexture), new Vector2(framebufferObject.Width, framebufferObject.Height));
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
            ResourceLayouts = [_resourceLayout],
            ShaderSet = new ShaderSetDescription(_vertexLayoutDescriptions, _shaders),
            Outputs = framebufferObject.Framebuffer.OutputDescription
        };

        pipeline?.Dispose();
        pipeline = _device.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
    }

    protected override void Destroy()
    {
        _sdfTexture.Dispose();
        _sdfTextureView.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _uniformBuffer.Dispose();
        _normalBuffer.Dispose();
        _resourceLayout.Dispose();
        _resourceSet.Dispose();

        foreach (Shader shader in _shaders)
        {
            shader.Dispose();
        }

        _commandList.Dispose();

        framebufferObject?.Dispose();
        pipeline?.Dispose();
    }
}
