using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;

namespace Renderer.Components;

internal abstract class Scene : DisposableObject
{
    protected readonly GraphicsDevice _graphicsDevice;
    protected readonly ResourceFactory _resourceFactory;
    protected readonly ImGuiController _imGuiController;
    protected readonly FBO _fbo;
    protected readonly CommandList _commandList;

    private nint _presentTextureHandle;

    protected Scene(MainWindow mainWindow)
    {
        _graphicsDevice = mainWindow.GraphicsDevice;
        _resourceFactory = mainWindow.ResourceFactory;
        _imGuiController = mainWindow.ImGuiController;
        _fbo = new FBO(_resourceFactory, InitialColorFormat(), InitialDepthFormat(), InitialSampleCount());
        _commandList = _resourceFactory.CreateGraphicsCommandList();

        Initialize();
    }

    public string Title { get; set; } = string.Empty;

    public uint Width { get; private set; }

    public uint Height { get; private set; }

    public bool IsVisible { get; private set; }

    public bool IsHovered { get; private set; }

    public bool IsFocused { get; private set; }

    public bool IsLeftClicked { get; private set; }

    public bool IsRightClicked { get; private set; }

    public bool IsMiddleClicked { get; private set; }

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

            if (_fbo.Resize(Width, Height) && _fbo.IsReady)
            {
                _presentTextureHandle = _imGuiController.GetOrCreateImGuiBinding(_resourceFactory, _fbo.PresentTexture!);
            }

            if (_fbo.IsReady)
            {
                _commandList.Begin();
                {
                    _commandList.SetFramebuffer(_fbo.Framebuffer!);

                    RenderCore(e);

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

    protected override void Destroy()
    {
        _fbo?.Dispose();
        _commandList.Dispose();
    }

    protected virtual PixelFormat InitialColorFormat() => PixelFormat.R8G8B8A8UNorm;

    protected virtual PixelFormat InitialDepthFormat() => PixelFormat.D32FloatS8UInt;

    protected virtual TextureSampleCount InitialSampleCount() => TextureSampleCount.Count1;

    protected abstract void Initialize();

    protected abstract void UpdateCore(UpdateEventArgs e);

    protected abstract void RenderCore(RenderEventArgs e);

}
