using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;

namespace Renderer.Components;

internal abstract class SubScene(GraphicsDevice graphicsDevice, ImGuiController imGuiController) : DisposableObject
{
    private readonly FBO _fbo = new(graphicsDevice.ResourceFactory);
    private readonly CommandList _commandList = graphicsDevice.ResourceFactory.CreateGraphicsCommandList();

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

            Initialize(Width, Height, SampleCount);

            if (_fbo.IsReady)
            {
                _commandList.Begin();
                {
                    RenderCore(_commandList, _fbo.Framebuffer!, e);

                    _fbo.Present(_commandList);
                }
                _commandList.End();

                graphicsDevice.SubmitCommands(_commandList);

                ImGui.Image(_presentTextureHandle, size, Vector2.Zero, Vector2.One, Vector4.One, Vector4.Zero);
            }
            else
            {
                ImGui.Text("Frame is not ready.");
            }

            ImGui.End();
        }
    }

    protected abstract void UpdateCore(UpdateEventArgs e);

    protected abstract void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e);

    protected override void Destroy()
    {
        _fbo?.Dispose();
        _commandList.Dispose();
    }

    private void Initialize(uint width, uint height, TextureSampleCount sampleCount = TextureSampleCount.Count1)
    {
        if (_fbo.Initialize(width, height, sampleCount) && _fbo.IsReady)
        {
            _presentTextureHandle = imGuiController.GetOrCreateImGuiBinding(graphicsDevice.ResourceFactory, _fbo.PresentTexture!);
        }
    }
}
