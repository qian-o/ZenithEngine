using Graphics.Core.Window;
using Graphics.Vulkan;
using Tests.AndroidApp.Controls;
using Tests.AndroidApp.Samples;
using Tests.AndroidApp.ViewModels;

namespace Tests.AndroidApp.Views;

public partial class SamplePage : ShellPage
{
    public static readonly BindableProperty SampleProperty = BindableProperty.Create(nameof(Sample), typeof(ISample), typeof(SamplePage), null);

    private readonly CommandList _commandList;

    public SamplePage()
    {
        InitializeComponent();

        BindingContext = new SampleViewModel();

        _commandList = App.Device.Factory.CreateGraphicsCommandList();
    }

    ~SamplePage()
    {
        _commandList.Dispose();
    }

    public ISample? Sample
    {
        get { return (ISample)GetValue(SampleProperty); }
        set { SetValue(SampleProperty, value); }
    }

    private void Renderer_Initialized(object sender, EventArgs e)
    {
        Sample?.Load(Renderer.Swapchain, Camera);
    }

    private void Renderer_Update(object sender, UpdateEventArgs e)
    {
        Sample?.Update(Renderer.Swapchain, (float)Renderer.Width, (float)Renderer.Height, Camera, e.DeltaTime, e.TotalTime);
    }

    private void Renderer_Render(object sender, RenderEventArgs e)
    {
        if (Sample == null)
        {
            return;
        }

        _commandList.Begin();

        Sample?.Render(_commandList, Renderer.Swapchain, e.DeltaTime, e.TotalTime);

        _commandList.End();

        App.Device.SubmitCommandsAndSwapBuffers(_commandList, Renderer.Swapchain);
    }

    private void Renderer_Disposed(object sender, EventArgs e)
    {
        Sample?.Unload();
    }
}