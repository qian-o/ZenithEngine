using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;

namespace Renderer.Components;

internal abstract class Scene : MVVM
{
    protected readonly GraphicsDevice _graphicsDevice;
    protected readonly ResourceFactory _resourceFactory;
    protected readonly ImGuiController _imGuiController;

    private readonly CommandList _commandList;

    private FBO? _fbo;
    private uint _width;
    private uint _height;
    private bool _isVisible;
    private bool _isHovered;
    private bool _isFocused;
    private bool _isLeftClicked;
    private bool _isRightClicked;
    private bool _isMiddleClicked;
    private nint _presentTextureHandle = -1;

    protected Scene(MainWindow mainWindow)
    {
        _graphicsDevice = mainWindow.GraphicsDevice;
        _resourceFactory = mainWindow.ResourceFactory;
        _imGuiController = mainWindow.ImGuiController;

        _commandList = _resourceFactory.CreateGraphicsCommandList();

        Initialize();
    }

    public string Title { get; set; } = string.Empty;

    public uint Width => _width;

    public uint Height => _height;

    public bool IsVisible => _isVisible;

    public bool IsHovered => _isHovered;

    public bool IsFocused => _isFocused;

    public bool IsLeftClicked => _isLeftClicked;

    public bool IsRightClicked => _isRightClicked;

    public bool IsMiddleClicked => _isMiddleClicked;

    public void Update(UpdateEventArgs e)
    {
        UpdateCore(e);
    }

    public void Render(RenderEventArgs e)
    {
        string title = string.IsNullOrEmpty(Title) ? Id : Title;

        if (_isVisible = ImGui.Begin(title))
        {
            ImGui.SetWindowSize(new Vector2(100, 100), ImGuiCond.FirstUseEver);

            Vector2 size = ImGui.GetContentRegionAvail();

            _width = (uint)Math.Max(0, Convert.ToInt32(size.X));
            _height = (uint)Math.Max(0, Convert.ToInt32(size.Y));

            _isHovered = ImGui.IsWindowHovered();
            _isFocused = ImGui.IsWindowFocused();
            _isLeftClicked = ImGui.IsMouseClicked(ImGuiMouseButton.Left);
            _isRightClicked = ImGui.IsMouseClicked(ImGuiMouseButton.Right);
            _isMiddleClicked = ImGui.IsMouseClicked(ImGuiMouseButton.Middle);

            if (_width != 0 && _height != 0)
            {
                if (_fbo == null || _fbo.Width != _width || _fbo.Height != _height || _fbo.SampleCount != App.Settings.SampleCount)
                {
                    bool isRecreatePipelineRequired = _fbo == null || _fbo.SampleCount != App.Settings.SampleCount;

                    _fbo?.Dispose();
                    _imGuiController.RemoveImGuiBinding(_presentTextureHandle);

                    _fbo = new FBO(_resourceFactory, _width, _height, sampleCount: App.Settings.SampleCount);
                    _presentTextureHandle = _imGuiController.GetOrCreateImGuiBinding(_resourceFactory, _fbo.PresentTexture);

                    if (isRecreatePipelineRequired)
                    {
                        RecreatePipeline(_fbo.Framebuffer);
                    }
                }

                _commandList.Begin();
                {
                    _commandList.SetFramebuffer(_fbo.Framebuffer);

                    RenderCore(_commandList, _fbo.Framebuffer, e);

                    _fbo.Present(_commandList);
                }
                _commandList.End();

                _graphicsDevice.SubmitCommands(_commandList);

                ImGui.Image(_presentTextureHandle, size, Vector2.Zero, Vector2.One, Vector4.One, Vector4.Zero);
            }
            else
            {
                ImGui.Text("Frame buffer is not created.");
            }

            ImGui.End();
        }
    }

    protected override void Destroy()
    {
        _fbo?.Dispose();
        _commandList.Dispose();
    }

    protected abstract void Initialize();

    protected abstract void RecreatePipeline(Framebuffer framebuffer);

    protected abstract void UpdateCore(UpdateEventArgs e);

    protected abstract void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e);
}
