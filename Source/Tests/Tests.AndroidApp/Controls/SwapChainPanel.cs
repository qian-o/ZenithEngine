using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using SharpGLTF.Schema2;
using Silk.NET.Core.Contexts;

namespace Tests.AndroidApp.Controls;

internal interface ISwapChainPanel
{
    Context Context { get; }

    GraphicsDevice Device { get; }

    void CreateSwapChainPanel(IVkSurface surface);

    void DestroySwapChainPanel();

    void Update();

    void Render();

    void Resize(uint width, uint height);
}

internal sealed class SwapChainPanel : View, ISwapChainPanel
{
    private readonly CommandList _commandList;

    private Swapchain? _swapchain;

    public SwapChainPanel()
    {
        _commandList = Device.Factory.CreateGraphicsCommandList();

        string assetPath = "Assets/Models/Sponza/glTF";
        ModelRoot root = ModelRoot.Load("Sponza.gltf", ReadContext.Create(FileReader));

        assetPath = "Assets/Shaders";
        Shader vs = Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, [.. FileReader("GLTF.hlsl.spv")], "mainVS"));
        Shader fs = Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, [.. FileReader("GLTF.hlsl.spv")], "mainFS"));

        ArraySegment<byte> FileReader(string assetName)
        {
            using Stream stream = FileSystem.OpenAppPackageFileAsync(Path.Combine(assetPath, assetName)).Result;

            using MemoryStream memoryStream = new();
            stream.CopyTo(memoryStream);

            return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }
    }

    public Context Context => App.Context;

    public GraphicsDevice Device => App.Device;

    #region ISwapChainPanel
    void ISwapChainPanel.CreateSwapChainPanel(IVkSurface surface)
    {
        _swapchain = Device.Factory.CreateSwapchain(new SwapchainDescription(surface, Device.GetBestDepthFormat()));
    }

    void ISwapChainPanel.DestroySwapChainPanel()
    {
        _swapchain?.Dispose();

        _swapchain = null;
    }

    void ISwapChainPanel.Update()
    {
    }

    void ISwapChainPanel.Render()
    {
        if (_swapchain == null)
        {
            return;
        }

        _commandList.Begin();

        _commandList.SetFramebuffer(_swapchain.Framebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Red);
        _commandList.ClearDepthStencil(1.0f);

        _commandList.End();

        Device.SubmitCommandsAndSwapBuffers(_commandList, _swapchain);
    }

    void ISwapChainPanel.Resize(uint width, uint height)
    {
        _swapchain?.Resize();
    }
    #endregion
}
