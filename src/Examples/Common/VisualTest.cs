using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ImGuiWrapper;
using ZenithEngine.Windowing;
using ZenithEngine.Windowing.Interfaces;

namespace Common;

public abstract unsafe class VisualTest
{
    protected VisualTest(string name, Backend backend)
    {
        List<double> samples = [];

        Window = WindowController.CreateWindow(name);

        Context = GraphicsContext.Create(backend);

#if DEBUG
        Context.CreateDevice(true);
#else
        Context.CreateDevice();
#endif

        Window.Loaded += (a, b) =>
        {
            SwapChainDesc swapChainDesc = SwapChainDesc.Default(Window.Surface);

            SwapChain = Context.Factory.CreateSwapChain(in swapChainDesc);

            ImGuiController = new ImGuiController(Context,
                                                  SwapChain.FrameBuffer.Output,
                                                  Window,
                                                  fontConfig: new(Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "msyh.ttf"),
                                                                  18,
                                                                  static (io) => (nint)io.Fonts.GetGlyphRangesChineseSimplifiedCommon()));

            CommandProcessor = Context.Factory.CreateCommandProcessor(CommandProcessorType.Graphics);

            OnLoad();
        };

        Window.Update += (a, b) =>
        {
            ImGuiController.Update(b.DeltaTime, Window.Size);

            OnUpdate(b.DeltaTime, b.TotalTime);
        };

        Window.Render += (a, b) =>
        {
            OnRender(b.DeltaTime, b.TotalTime);

            CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

            commandBuffer.Begin();

            ImGuiController.PrepareResources(commandBuffer);

            commandBuffer.BeginRendering(SwapChain.FrameBuffer, new(1, options: ClearOptions.None));

            ImGuiController.Render(commandBuffer);

            commandBuffer.EndRendering();

            commandBuffer.End();

            commandBuffer.Commit();

            CommandProcessor.Submit();
            CommandProcessor.WaitIdle();

            SwapChain.Present();

            samples.Add(b.DeltaTime);

            if (samples.Count > 100)
            {
                Window.Title = $"{name} - {1.0 / samples.Average():F2} FPS";

                samples.Clear();
            }
        };

        Window.SizeChanged += (a, b) =>
        {
            SwapChain.Resize();
        };

        Window.Unloaded += (a, b) =>
        {
            OnDestroy();

            CommandProcessor.Dispose();
            ImGuiController.Dispose();
            SwapChain.Dispose();
            Context.Dispose();
        };
    }

    public IWindow Window { get; }

    public GraphicsContext Context { get; }

    public SwapChain SwapChain { get; private set; } = null!;

    public ImGuiController ImGuiController { get; private set; } = null!;

    public CommandProcessor CommandProcessor { get; private set; } = null!;

    public void Run()
    {
        Window.Show();

        WindowController.Loop(true);
    }

    protected abstract void OnLoad();

    protected abstract void OnUpdate(double deltaTime, double totalTime);

    protected abstract void OnRender(double deltaTime, double totalTime);

    protected abstract void OnDestroy();
}
