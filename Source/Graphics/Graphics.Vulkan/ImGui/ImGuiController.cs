using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Silk.NET.Input;

namespace Graphics.Vulkan;

public unsafe class ImGuiController : DisposableObject
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

    private static readonly Key[] _keyEnumArr = (Key[])Enum.GetValues(typeof(Key));

    private readonly GraphicsWindow _graphicsWindow;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ResourceFactory _factory;
    private readonly ColorSpaceHandling _colorSpaceHandling;
    private readonly ImGuiContextPtr _imGuiContext;
    private readonly List<char> _pressedChars = [];

    #region Resource Management
    private readonly object _lock = new();
    private readonly Dictionary<TextureView, nint> _mapped = [];
    private readonly Dictionary<nint, ResourceSet> _selfSets = [];
    private readonly Dictionary<nint, TextureView> _selfViews = [];
    #endregion

    private int _windowWidth;
    private int _windowHeight;

    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _uboBuffer = null!;
    private ResourceLayout _layout0 = null!;
    private ResourceLayout _layout1 = null!;
    private ResourceSet _resourceSet = null!;
    private Pipeline _pipeline = null!;
    private Texture _fontTexture = null!;

    private bool _frameBegun;
    private IKeyboard _keyboard = null!;

    #region Constructors
    public ImGuiController(GraphicsWindow graphicsWindow,
                           GraphicsDevice graphicsDevice,
                           ColorSpaceHandling colorSpaceHandling,
                           ImGuiFontConfig? imGuiFontConfig,
                           Action? onConfigureIO)
    {
        _graphicsWindow = graphicsWindow;
        _graphicsDevice = graphicsDevice;
        _factory = graphicsDevice.ResourceFactory;
        _colorSpaceHandling = colorSpaceHandling;
        _imGuiContext = ImGui.CreateContext();
        _windowWidth = graphicsWindow.Width;
        _windowHeight = graphicsWindow.Height;

        Initialize(imGuiFontConfig, onConfigureIO);
    }

    public ImGuiController(GraphicsWindow graphicsWindow,
                           GraphicsDevice graphicsDevice,
                           ImGuiFontConfig imGuiFontConfig) : this(graphicsWindow,
                                                                   graphicsDevice,
                                                                   ColorSpaceHandling.Legacy,
                                                                   imGuiFontConfig,
                                                                   null)
    {
    }

    public ImGuiController(GraphicsWindow graphicsWindow,
                           GraphicsDevice graphicsDevice,
                           Action onConfigureIO) : this(graphicsWindow,
                                                        graphicsDevice,
                                                        ColorSpaceHandling.Legacy,
                                                        null,
                                                        onConfigureIO)
    {
    }

    public ImGuiController(GraphicsWindow graphicsWindow,
                           GraphicsDevice graphicsDevice) : this(graphicsWindow,
                                                                 graphicsDevice,
                                                                 ColorSpaceHandling.Legacy,
                                                                 null,
                                                                 null)
    {
    }
    #endregion

    public void SwapExtraWindows(GraphicsDevice gd)
    {
        ImVectorImGuiViewportPtr viewports = ImGui.GetPlatformIO().Viewports;
        for (int i = 0; i < viewports.Size; i++)
        {
            ImGuiViewport* viewport = viewports.Data[i];

            PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

            ImGuiWindow imGuiWindow = platformUserData->GetImGuiWindow();

            if (imGuiWindow.Swapchain != null)
            {
                gd.SwapBuffers(imGuiWindow.Swapchain);
            }
        }
    }

    public void Render(CommandList commandList)
    {
        if (_frameBegun)
        {
            ImGuiContextPtr currentContext = ImGui.GetCurrentContext();

            if (currentContext != _imGuiContext)
            {
                ImGui.SetCurrentContext(_imGuiContext);
                ImGuizmo.SetImGuiContext(_imGuiContext);
            }

            _frameBegun = false;

            ImGui.Render();
            RenderImDrawData(commandList, ImGui.GetDrawData());

            ImGui.UpdatePlatformWindows();

            ImVectorImGuiViewportPtr viewports = ImGui.GetPlatformIO().Viewports;
            for (int i = 0; i < viewports.Size; i++)
            {
                ImGuiViewport* viewport = viewports.Data[i];

                PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

                ImGuiWindow imGuiWindow = platformUserData->GetImGuiWindow();

                if (imGuiWindow.Swapchain != null)
                {
                    commandList.SetFramebuffer(imGuiWindow.Swapchain.Framebuffer);

                    RenderImDrawData(commandList, viewport->DrawData);
                }
            }

            if (currentContext != _imGuiContext)
            {
                ImGui.SetCurrentContext(currentContext);
                ImGuizmo.SetImGuiContext(currentContext);
            }
        }
    }

    public void Update(float deltaSeconds)
    {
        ImGuiContextPtr currentContext = ImGui.GetCurrentContext();
        if (currentContext != _imGuiContext)
        {
            ImGui.SetCurrentContext(_imGuiContext);
            ImGuizmo.SetImGuiContext(_imGuiContext);
        }

        if (_frameBegun)
        {
            ImGui.Render();
            ImGui.UpdatePlatformWindows();
        }

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput();

        _frameBegun = true;

        ImGui.NewFrame();
        ImGuizmo.BeginFrame();

        if (currentContext != _imGuiContext)
        {
            ImGui.SetCurrentContext(currentContext);
            ImGuizmo.SetImGuiContext(currentContext);
        }
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

    protected override void Destroy()
    {
        foreach (ResourceSet resourceSet in _selfSets.Values)
        {
            resourceSet.Dispose();
        }
        foreach (TextureView textureView in _selfViews.Values)
        {
            textureView.Dispose();
        }
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _uboBuffer.Dispose();
        _layout0.Dispose();
        _layout1.Dispose();
        _resourceSet.Dispose();
        _pipeline.Dispose();
        _fontTexture.Dispose();

        ImGui.DestroyPlatformWindows();

        ImGui.DestroyContext(_imGuiContext);
    }

    private ResourceSet GetResourceSet(nint handle)
    {
        if (!_selfSets.TryGetValue(handle, out ResourceSet? resourceSet))
        {
            throw new InvalidOperationException("No resource set found for the given handle.");
        }

        return resourceSet;
    }

    private static bool TryMapKey(Key key, out ImGuiKey result)
    {
        static ImGuiKey KeyToImGuiKeyShortcut(Key keyToConvert, Key startKey1, ImGuiKey startKey2)
        {
            int changeFromStart1 = (int)keyToConvert - (int)startKey1;
            return startKey2 + changeFromStart1;
        }

        result = key switch
        {
            >= Key.F1 and <= Key.F24 => KeyToImGuiKeyShortcut(key, Key.F1, ImGuiKey.F1),
            >= Key.Keypad0 and <= Key.Keypad9 => KeyToImGuiKeyShortcut(key, Key.Keypad0, ImGuiKey.Keypad0),
            >= Key.A and <= Key.Z => KeyToImGuiKeyShortcut(key, Key.A, ImGuiKey.A),
            >= Key.Number0 and <= Key.Number9 => KeyToImGuiKeyShortcut(key, Key.Number0, ImGuiKey.Key0),
            Key.ShiftLeft or Key.ShiftRight => ImGuiKey.ModShift,
            Key.ControlLeft or Key.ControlRight => ImGuiKey.ModCtrl,
            Key.AltLeft or Key.AltRight => ImGuiKey.ModAlt,
            Key.SuperLeft or Key.SuperRight => ImGuiKey.ModSuper,
            Key.Menu => ImGuiKey.Menu,
            Key.Up => ImGuiKey.UpArrow,
            Key.Down => ImGuiKey.DownArrow,
            Key.Left => ImGuiKey.LeftArrow,
            Key.Right => ImGuiKey.RightArrow,
            Key.Enter => ImGuiKey.Enter,
            Key.Escape => ImGuiKey.Escape,
            Key.Space => ImGuiKey.Space,
            Key.Tab => ImGuiKey.Tab,
            Key.Backspace => ImGuiKey.Backspace,
            Key.Insert => ImGuiKey.Insert,
            Key.Delete => ImGuiKey.Delete,
            Key.PageUp => ImGuiKey.PageUp,
            Key.PageDown => ImGuiKey.PageDown,
            Key.Home => ImGuiKey.Home,
            Key.End => ImGuiKey.End,
            Key.CapsLock => ImGuiKey.CapsLock,
            Key.ScrollLock => ImGuiKey.ScrollLock,
            Key.PrintScreen => ImGuiKey.PrintScreen,
            Key.Pause => ImGuiKey.Pause,
            Key.NumLock => ImGuiKey.NumLock,
            Key.KeypadDivide => ImGuiKey.KeypadDivide,
            Key.KeypadMultiply => ImGuiKey.KeypadMultiply,
            Key.KeypadSubtract => ImGuiKey.KeypadSubtract,
            Key.KeypadAdd => ImGuiKey.KeypadAdd,
            Key.KeypadDecimal => ImGuiKey.KeypadDecimal,
            Key.KeypadEnter => ImGuiKey.KeypadEnter,
            Key.GraveAccent => ImGuiKey.GraveAccent,
            Key.Minus => ImGuiKey.Minus,
            Key.Equal => ImGuiKey.Equal,
            Key.LeftBracket => ImGuiKey.LeftBracket,
            Key.RightBracket => ImGuiKey.RightBracket,
            Key.Semicolon => ImGuiKey.Semicolon,
            Key.Apostrophe => ImGuiKey.Apostrophe,
            Key.Comma => ImGuiKey.Comma,
            Key.Period => ImGuiKey.Period,
            Key.Slash => ImGuiKey.Slash,
            Key.BackSlash => ImGuiKey.Backslash,
            _ => ImGuiKey.None
        };

        return result != ImGuiKey.None;
    }

    private void UpdateImGuiInput()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        IMouse mouse = _graphicsWindow.InputContext.Mice[0];
        IKeyboard keyboard = _graphicsWindow.InputContext.Keyboards[0];

        ScrollWheel scrollWheel = mouse.ScrollWheels[0];

        io.AddMousePosEvent(_graphicsWindow.X + mouse.Position.X, _graphicsWindow.Y + mouse.Position.Y);
        io.AddMouseButtonEvent(0, mouse.IsButtonPressed(MouseButton.Left));
        io.AddMouseButtonEvent(1, mouse.IsButtonPressed(MouseButton.Right));
        io.AddMouseButtonEvent(2, mouse.IsButtonPressed(MouseButton.Middle));
        io.AddMouseButtonEvent(3, mouse.IsButtonPressed(MouseButton.Button4));
        io.AddMouseButtonEvent(4, mouse.IsButtonPressed(MouseButton.Button5));
        io.AddMouseWheelEvent(scrollWheel.X, scrollWheel.Y);

        foreach (char pressedChar in _pressedChars)
        {
            io.AddInputCharacter(pressedChar);
        }

        foreach (Key key in _keyEnumArr)
        {
            if (key != Key.Unknown && TryMapKey(key, out ImGuiKey imGuiKey))
            {
                io.AddKeyEvent(imGuiKey, keyboard.IsKeyPressed(key));
            }
        }

        _pressedChars.Clear();

        ImVectorImGuiViewportPtr viewports = ImGui.GetPlatformIO().Viewports;
        for (int i = 0; i < viewports.Size; i++)
        {
            ImGuiViewport* viewport = viewports.Data[i];

            PlatformUserData* platformUserData = (PlatformUserData*)viewport->PlatformUserData;

            platformUserData->GetImGuiWindow().DoEvents();
        }
    }

    private void RenderImDrawData(CommandList commandList, ImDrawDataPtr drawDataPtr)
    {
        if (drawDataPtr.CmdListsCount == 0)
        {
            return;
        }

        ImGuiIOPtr io = ImGui.GetIO();

        uint totalVBSize = (uint)(drawDataPtr.TotalVtxCount * sizeof(ImDrawVert));
        if (totalVBSize > _vertexBuffer.SizeInBytes)
        {
            _vertexBuffer.Dispose();
            _vertexBuffer = _factory.CreateBuffer(new BufferDescription((uint)(totalVBSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        }

        uint totalIBSize = (uint)(drawDataPtr.TotalIdxCount * sizeof(ushort));
        if (totalIBSize > _indexBuffer.SizeInBytes)
        {
            _indexBuffer.Dispose();
            _indexBuffer = _factory.CreateBuffer(new BufferDescription((uint)(totalIBSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        }


        Vector2 displayPos = drawDataPtr.DisplayPos;

        // Update vertex and index buffers
        {
            ImDrawVert* vertMap = (ImDrawVert*)_vertexBuffer.Map(totalVBSize);
            ushort* idxMap = (ushort*)_indexBuffer.Map(totalIBSize);

            for (int i = 0; i < drawDataPtr.CmdListsCount; i++)
            {
                ImDrawListPtr imDrawListPtr = drawDataPtr.CmdLists.Data[i];

                Unsafe.CopyBlock(vertMap,
                                 imDrawListPtr.VtxBuffer.Data,
                                 (uint)(imDrawListPtr.VtxBuffer.Size * sizeof(ImDrawVert)));

                Unsafe.CopyBlock(idxMap,
                                 imDrawListPtr.IdxBuffer.Data,
                                 (uint)(imDrawListPtr.IdxBuffer.Size * sizeof(ushort)));

                vertMap += imDrawListPtr.VtxBuffer.Size;
                idxMap += imDrawListPtr.IdxBuffer.Size;
            }

            _vertexBuffer.Unmap();
            _indexBuffer.Unmap();
        }

        // Orthographic projection matrix
        {
            void* uboMap = _uboBuffer.Map((uint)sizeof(Matrix4x4));

            Matrix4x4 orthoProjection = Matrix4x4.CreateOrthographicOffCenter(displayPos.X,
                                                                              displayPos.X + io.DisplaySize.X,
                                                                              displayPos.Y + io.DisplaySize.Y,
                                                                              displayPos.Y,
                                                                              -1.0f,
                                                                              1.0f);

            Unsafe.CopyBlock(uboMap, &orthoProjection, (uint)sizeof(Matrix4x4));

            _uboBuffer.Unmap();
        }

        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U16);
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _resourceSet);

        drawDataPtr.ScaleClipRects(io.DisplayFramebufferScale);

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

    private void OnKeyChar(IKeyboard arg1, char arg2)
    {
        _pressedChars.Add(arg2);
    }

    private void WindowResized(object? sender, ResizeEventArgs e)
    {
        _windowWidth = (int)e.Width;
        _windowHeight = (int)e.Height;
    }

    private void BeginFrame()
    {
        ImGui.NewFrame();
        ImGuizmo.BeginFrame();

        _frameBegun = true;
        _keyboard = _graphicsWindow.InputContext.Keyboards[0];

        _graphicsWindow.Resize += WindowResized;
        _keyboard.KeyChar += OnKeyChar;
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.DisplaySize = new Vector2(_windowWidth, _windowHeight);

        if (_windowWidth > 0 && _windowHeight > 0)
        {
            io.DisplayFramebufferScale = new Vector2(_graphicsWindow.FramebufferWidth / _windowWidth, _graphicsWindow.FramebufferHeight / _windowHeight);
        }

        io.DeltaTime = deltaSeconds;
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

    private void CreateDeviceResources()
    {
        _vertexBuffer = _factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        _indexBuffer = _factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        _uboBuffer = _factory.CreateBuffer(new BufferDescription((uint)sizeof(Matrix4x4), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

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
                                                 [new SpecializationConstant(0, _colorSpaceHandling == ColorSpaceHandling.Legacy)]),
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

    private void Initialize(ImGuiFontConfig? imGuiFontConfig, Action? onConfigureIO)
    {
        ImGui.SetCurrentContext(_imGuiContext);
        ImGui.StyleColorsDark();

        ImGuizmo.SetImGuiContext(_imGuiContext);

        ImGuiIOPtr io = ImGui.GetIO();

        if (imGuiFontConfig.HasValue)
        {
            nint glyph_ranges = imGuiFontConfig.Value.GetGlyphRange?.Invoke(io) ?? 0;
            io.Fonts.AddFontFromFileTTF(imGuiFontConfig.Value.FontPath, imGuiFontConfig.Value.FontSize, null, (char*)glyph_ranges);
        }

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        onConfigureIO?.Invoke();

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports;

        ImGuiPlatform.Initialize(_graphicsWindow, _graphicsDevice);

        CreateDeviceResources();
        SetPerFrameImGuiData(1.0f / 60.0f);
        BeginFrame();
    }
}
