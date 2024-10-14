using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using SharpGLTF.Schema2;

namespace Tests.AndroidApp;

public partial class MainPage : ContentPage
{
    private int count;
    private CommandList? _commandList;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        CounterBtn.Text = count == 1 ? $"Clicked {count} time" : $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private void SwapChainPanelLoaded(object sender, EventArgs e)
    {
        _commandList = App.Device.Factory.CreateGraphicsCommandList();

        string assetPath = "Assets/Models/Sponza/glTF";
        ModelRoot root = ModelRoot.Load("Sponza.gltf", ReadContext.Create(FileReader));

        assetPath = "Assets/Shaders";
        Shader vs = App.Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, [.. FileReader("GLTF.vs.hlsl.spv")], "main"));
        Shader fs = App.Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, [.. FileReader("GLTF.ps.hlsl.spv")], "main"));

        ArraySegment<byte> FileReader(string assetName)
        {
            using Stream stream = FileSystem.OpenAppPackageFileAsync(Path.Combine(assetPath, assetName)).Result;

            using MemoryStream memoryStream = new();
            stream.CopyTo(memoryStream);

            return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }
    }

    private void SwapChainPanelUpdate(object sender, UpdateEventArgs e)
    {

    }

    private void SwapChainPanelRender(object sender, RenderEventArgs e)
    {
        if (_commandList == null)
        {
            return;
        }

        _commandList.Begin();

        _commandList.SetFramebuffer(Renderer.Swapchain.Framebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Red);
        _commandList.ClearDepthStencil(1.0f);

        _commandList.End();

        App.Device.SubmitCommandsAndSwapBuffers(_commandList, Renderer.Swapchain);
    }
}

