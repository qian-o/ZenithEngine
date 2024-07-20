using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;

namespace Renderer.Components;

internal abstract class Scene : DisposableObject
{
    protected readonly GraphicsDevice _graphicsDevice;
    protected readonly ImGuiController _imGuiController;

    private readonly FBO _fbo;
    private readonly CommandList _commandList;

    protected Scene(MainWindow mainWindow)
    {
        _graphicsDevice = mainWindow.GraphicsDevice;
        _imGuiController = mainWindow.ImGuiController;

        _fbo = new FBO(mainWindow.GraphicsDevice.ResourceFactory);
        _commandList = mainWindow.GraphicsDevice.ResourceFactory.CreateGraphicsCommandList();

        Initialize();
    }

    private nint _presentTextureHandle;

    public string Title { get; set; } = string.Empty;

    public uint Width { get; private set; }

    public uint Height { get; private set; }

    public bool IsVisible { get; private set; }

    public bool IsHovered { get; private set; }

    public bool IsFocused { get; private set; }

    public bool IsLeftClicked { get; private set; }

    public bool IsRightClicked { get; private set; }

    public bool IsMiddleClicked { get; private set; }

    public TextureSampleCount SampleCount { get; set; } = TextureSampleCount.Count1;

    public void Update(UpdateEventArgs e)
    {
        UpdateCore(e);
    }

    public void Render(RenderEventArgs e)
    {
        string title = string.IsNullOrEmpty(Title) ? Id : Title;

        if (IsVisible = ImGui.Begin(title))
        {
            Vector2 size = ImGui.GetContentRegionAvail();

            Width = (uint)Math.Max(0, Convert.ToInt32(size.X));
            Height = (uint)Math.Max(0, Convert.ToInt32(size.Y));

            IsHovered = ImGui.IsWindowHovered();
            IsFocused = ImGui.IsWindowFocused();
            IsLeftClicked = ImGui.IsMouseClicked(ImGuiMouseButton.Left);
            IsRightClicked = ImGui.IsMouseClicked(ImGuiMouseButton.Right);
            IsMiddleClicked = ImGui.IsMouseClicked(ImGuiMouseButton.Middle);

            UpdateFBO(Width, Height, SampleCount);

            if (_fbo.IsReady)
            {
                _commandList.Begin();
                {
                    _commandList.SetFramebuffer(_fbo.Framebuffer!);

                    RenderCore(_commandList, _fbo.Framebuffer!, e);

                    _fbo.Present(_commandList);
                }
                _commandList.End();

                _graphicsDevice.SubmitCommands(_commandList);

                ImGui.Image(_presentTextureHandle, size, Vector2.Zero, Vector2.One, Vector4.One, Vector4.Zero);
            }
            else
            {
                ImGui.Text("Frame is not ready.");
            }

            ImGui.End();
        }
    }

    protected abstract void Initialize();

    protected abstract void UpdateCore(UpdateEventArgs e);

    protected abstract void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e);

    protected override void Destroy()
    {
        _fbo?.Dispose();
        _commandList.Dispose();
    }

    private void UpdateFBO(uint width, uint height, TextureSampleCount sampleCount = TextureSampleCount.Count1)
    {
        if (_fbo.Update(width, height, sampleCount) && _fbo.IsReady)
        {
            _presentTextureHandle = _imGuiController.GetOrCreateImGuiBinding(_graphicsDevice.ResourceFactory, _fbo.PresentTexture!);
        }
    }
}
