using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Hexa.NET.ImGui;

namespace Graphics.Vulkan;

internal sealed unsafe class ImGuiRenderer : DisposableObject
{
    private const string VertexShader = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec4 Color;

layout(location = 0) out struct
{
    vec2 UV;
    vec4 Color;
}fsin;

layout(set = 0, std140, binding = 0) uniform UBO
{
    mat4 Projection;
}ubo;

layout (constant_id = 0) const bool UseLegacyColorSpaceHandling = false;

vec3 SrgbToLinear(vec3 srgb)
{
    return srgb * (srgb * (srgb * 0.305306011 + 0.682171111) + 0.012522878);
}

void main()
{
    gl_Position = ubo.Projection * vec4(Position, 0, 1);
    fsin.UV = UV;
    fsin.Color = Color;
    if (UseLegacyColorSpaceHandling)
    {
        fsin.Color.rgb = SrgbToLinear(fsin.Color.rgb);
    }
    else
    {
        fsin.Color = fsin.Color;
    }
}";

    private const string FragmentShader = @"
#version 450

layout(location = 0) in struct
{
    vec2 UV;
    vec4 Color;
}fsin;

layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 1) uniform sampler FontSampler;
layout(set = 1, binding = 0) uniform texture2D FontTexture;

void main()
{
    fsout_Color = fsin.Color * texture(sampler2D(FontTexture, FontSampler), fsin.UV.st);
}";

    private readonly GraphicsDevice _graphicsDevice;
    private readonly ResourceFactory _factory;

    #region Resource Management
    private readonly object _lock = new();
    private readonly Dictionary<TextureView, nint> _mapped = [];
    private readonly Dictionary<nint, ResourceSet> _selfSets = [];
    private readonly Dictionary<nint, TextureView> _selfViews = [];
    #endregion

    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _uboBuffer = null!;
    private ResourceLayout _layout0 = null!;
    private ResourceLayout _layout1 = null!;
    private ResourceSet _resourceSet = null!;
    private Pipeline _pipeline = null!;
    private Texture _fontTexture = null!;

    public ImGuiRenderer(GraphicsDevice graphicsDevice, ColorSpaceHandling colorSpaceHandling)
    {
        _graphicsDevice = graphicsDevice;
        _factory = graphicsDevice.ResourceFactory;

        CreateDeviceResources(colorSpaceHandling);
    }

    public nint GetOrCreateImGuiBinding(ResourceFactory factory, TextureView textureView)
    {
        lock (_lock)
        {
            if (!_mapped.TryGetValue(textureView, out nint binding))
            {
                while (_selfSets.ContainsKey(binding))
                {
                    binding++;
                }

                _mapped[textureView] = binding;
                _selfSets[binding] = factory.CreateResourceSet(new ResourceSetDescription(_layout1, textureView));
            }

            return binding;
        }
    }

    public nint GetOrCreateImGuiBinding(ResourceFactory factory, Texture texture)
    {
        lock (_lock)
        {
            TextureView textureView = factory.CreateTextureView(texture);

            nint binding = GetOrCreateImGuiBinding(factory, textureView);

            _selfViews[binding] = textureView;

            return binding;
        }
    }

    public void RemoveImGuiBinding(nint binding)
    {
        lock (_lock)
        {
            if (_selfSets.TryGetValue(binding, out ResourceSet? resourceSet))
            {
                resourceSet.Dispose();
                _selfSets.Remove(binding);
            }

            if (_selfViews.TryGetValue(binding, out TextureView? textureView))
            {
                textureView.Dispose();
                _selfViews.Remove(binding);
            }

            if (textureView != null)
            {
                _mapped.Remove(textureView);
            }
        }
    }

    public void RenderImDrawData(CommandList commandList, ImDrawDataPtr drawDataPtr)
    {
        if (drawDataPtr.CmdListsCount == 0)
        {
            return;
        }

        uint totalVBSize = (uint)(drawDataPtr.TotalVtxCount * sizeof(ImDrawVert));
        if (totalVBSize > _vertexBuffer.SizeInBytes)
        {
            _vertexBuffer.Dispose();
            _vertexBuffer = _factory.CreateBuffer(new BufferDescription((uint)(totalVBSize * 1.5f), BufferUsage.VertexBuffer));
        }

        uint totalIBSize = (uint)(drawDataPtr.TotalIdxCount * sizeof(ushort));
        if (totalIBSize > _indexBuffer.SizeInBytes)
        {
            _indexBuffer.Dispose();
            _indexBuffer = _factory.CreateBuffer(new BufferDescription((uint)(totalIBSize * 1.5f), BufferUsage.IndexBuffer));
        }

        Vector2 displayPos = drawDataPtr.DisplayPos;
        Vector2 displaySize = drawDataPtr.DisplaySize;

        // Update vertex and index buffers
        {
            uint vertOffset = 0;
            uint idxOffset = 0;
            for (int i = 0; i < drawDataPtr.CmdListsCount; i++)
            {
                ImDrawListPtr imDrawListPtr = drawDataPtr.CmdLists.Data[i];

                commandList.UpdateBuffer(_vertexBuffer,
                                         vertOffset,
                                         imDrawListPtr.VtxBuffer.Data,
                                         imDrawListPtr.VtxBuffer.Size);

                commandList.UpdateBuffer(_indexBuffer,
                                         idxOffset,
                                         imDrawListPtr.IdxBuffer.Data,
                                         imDrawListPtr.IdxBuffer.Size);

                vertOffset += (uint)(imDrawListPtr.VtxBuffer.Size * (uint)sizeof(ImDrawVert));
                idxOffset += (uint)(imDrawListPtr.IdxBuffer.Size * sizeof(ushort));
            }
        }

        // Orthographic projection matrix
        {
            Matrix4x4 orthoProjection = Matrix4x4.CreateOrthographicOffCenter(displayPos.X,
                                                                              displayPos.X + displaySize.X,
                                                                              displayPos.Y + displaySize.Y,
                                                                              displayPos.Y,
                                                                              -1.0f,
                                                                              1.0f);

            commandList.UpdateBuffer(_uboBuffer, 0, &orthoProjection, 1);
        }

        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U16);
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _resourceSet);

        drawDataPtr.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        int vertexOffset = 0;
        int indexOffset = 0;
        for (int i = 0; i < drawDataPtr.CmdListsCount; i++)
        {
            ImDrawListPtr imDrawListPtr = drawDataPtr.CmdLists.Data[i];

            for (int j = 0; j < imDrawListPtr.CmdBuffer.Size; j++)
            {
                ImDrawCmd imDrawCmd = imDrawListPtr.CmdBuffer.Data[j];

                if (imDrawCmd.UserCallback != null)
                {
                    ImDrawCallback callback = Marshal.GetDelegateForFunctionPointer<ImDrawCallback>((nint)imDrawCmd.UserCallback);
                    callback(imDrawListPtr, &imDrawCmd);
                }
                else
                {
                    commandList.SetGraphicsResourceSet(1, GetResourceSet(imDrawCmd.TextureId.Handle));

                    commandList.SetScissorRect(0,
                                               (uint)(imDrawCmd.ClipRect.X - displayPos.X),
                                               (uint)(imDrawCmd.ClipRect.Y - displayPos.Y),
                                               (uint)(imDrawCmd.ClipRect.Z - imDrawCmd.ClipRect.X),
                                               (uint)(imDrawCmd.ClipRect.W - imDrawCmd.ClipRect.Y));

                    commandList.DrawIndexed(imDrawCmd.ElemCount,
                                            1,
                                            (uint)(imDrawCmd.IdxOffset + indexOffset),
                                            (int)(imDrawCmd.VtxOffset + vertexOffset),
                                            0);
                }
            }

            vertexOffset += imDrawListPtr.VtxBuffer.Size;
            indexOffset += imDrawListPtr.IdxBuffer.Size;
        }
    }

    protected override void Destroy()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _uboBuffer.Dispose();
        _layout0.Dispose();
        _layout1.Dispose();
        _resourceSet.Dispose();
        _pipeline.Dispose();
        _fontTexture.Dispose();

        foreach (ResourceSet resourceSet in _selfSets.Values)
        {
            resourceSet.Dispose();
        }

        foreach (TextureView textureView in _selfViews.Values)
        {
            textureView.Dispose();
        }
    }

    private ResourceSet GetResourceSet(nint handle)
    {
        if (!_selfSets.TryGetValue(handle, out ResourceSet? resourceSet))
        {
            throw new InvalidOperationException("No resource set found for the given handle.");
        }

        return resourceSet;
    }

    private void RecreateFontDeviceTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        byte* pixels;
        int width;
        int height;
        io.Fonts.GetTexDataAsRGBA32(&pixels, &width, &height);

        TextureDescription description = TextureDescription.Texture2D((uint)width,
                                                                      (uint)height,
                                                                      1,
                                                                      PixelFormat.R8G8B8A8UNorm,
                                                                      TextureUsage.Sampled);

        _fontTexture = _factory.CreateTexture(description);

        _graphicsDevice.UpdateTexture(_fontTexture,
                                      pixels,
                                      width * height * 4,
                                      0,
                                      0,
                                      0,
                                      (uint)width,
                                      (uint)height,
                                      1,
                                      0,
                                      0);

        io.Fonts.SetTexID(GetOrCreateImGuiBinding(_factory, _fontTexture));
    }

    private void CreateDeviceResources(ColorSpaceHandling colorSpaceHandling)
    {
        _vertexBuffer = _factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer));
        _indexBuffer = _factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer));
        _uboBuffer = _factory.CreateBuffer(new BufferDescription((uint)sizeof(Matrix4x4), BufferUsage.UniformBuffer));

        Shader[] shaders = _factory.CreateFromSpirv(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexShader), "main"),
                                                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentShader), "main"));

        VertexLayoutDescription vertexLayoutDescription = new(new VertexElementDescription("Position", VertexElementFormat.Float2),
                                                              new VertexElementDescription("UV", VertexElementFormat.Float2),
                                                              new VertexElementDescription("Color", VertexElementFormat.Byte4Norm));

        ResourceLayoutDescription set0 = new(new ResourceLayoutElementDescription("UBO", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                                             new ResourceLayoutElementDescription("FontSampler", ResourceKind.Sampler, ShaderStages.Fragment));

        ResourceLayoutDescription set1 = new(new ResourceLayoutElementDescription("FontTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));

        _layout0 = _factory.CreateResourceLayout(set0);
        _layout1 = _factory.CreateResourceLayout(set1);
        _resourceSet = _factory.CreateResourceSet(new ResourceSetDescription(_layout0, _uboBuffer, _graphicsDevice.PointSampler));

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.Disabled,
            RasterizerState = RasterizerStateDescription.CullNone,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_layout0, _layout1],
            ShaderSet = new ShaderSetDescription([vertexLayoutDescription],
                                                 shaders,
                                                 [new SpecializationConstant(0, colorSpaceHandling == ColorSpaceHandling.Legacy)]),
            Outputs = _graphicsDevice.MainSwapchain.OutputDescription
        };

        _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);
        _pipeline.Name = "ImGui Pipeline";

        RecreateFontDeviceTexture();

        foreach (Shader shader in shaders)
        {
            shader.Dispose();
        }
    }
}
