using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;

namespace Renderer.Components;

internal abstract class SubScene(ResourceFactory resourceFactory, ImGuiController imGuiController) : DisposableObject
{
    private readonly FBO _fbo = new(resourceFactory);
    private readonly CommandList _commandList = resourceFactory.CreateGraphicsCommandList();

    private nint _presentTextureHandle;

    public string Title { get; set; } = string.Empty;

    public TextureSampleCount SampleCount { get; set; } = TextureSampleCount.Count1;

    public void Render()
    {
        string title = string.IsNullOrEmpty(Title) ? Id : Title;

        if (ImGui.Begin(title))
        {
            Vector2 size = ImGui.GetContentRegionAvail();

            Initialize(Convert.ToUInt32(size.X), Convert.ToUInt32(size.Y), SampleCount);

            if (_fbo.IsReady)
            {
                RenderCore(_commandList, _fbo.Framebuffer!);

                _fbo.Present(_commandList);

                ImGui.Image(_presentTextureHandle, size, Vector2.Zero, Vector2.One, Vector4.One, Vector4.Zero);
            }
            else
            {
                ImGui.Text("Frame is not ready.");
            }

            ImGui.End();
        }
    }

    protected abstract void RenderCore(CommandList commandList, Framebuffer framebuffer);

    protected override void Destroy()
    {
        _fbo?.Dispose();
        _commandList.Dispose();
    }

    private void Initialize(uint width, uint height, TextureSampleCount sampleCount = TextureSampleCount.Count1)
    {
        _fbo.Initialize(width, height, sampleCount);

        if (_fbo.IsReady)
        {
            _presentTextureHandle = imGuiController.GetOrCreateImGuiBinding(resourceFactory, _fbo.PresentTexture!);
        }
    }
}
