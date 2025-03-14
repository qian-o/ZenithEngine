using Hexa.NET.ImGui;
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
        Window = WindowController.CreateWindow(name, 1270, 720);

        Camera = new(Window);

        Context = GraphicsContext.Create(backend);

#if DEBUG
        Context.CreateDevice(true);
#else
        Context.CreateDevice();
#endif

        List<double> avgFrameTimes = [];

        double fps = 0;
        double lastTotalTime = 0;

        Window.Loaded += (a, b) =>
        {
            Window.Center();

            SwapChainDesc swapChainDesc = new(Window.Surface);

            SwapChain = Context.Factory.CreateSwapChain(in swapChainDesc);

            ImGuiController = new(Context,
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
            Camera.Update(b.DeltaTime, Window.Size);

            ImGuiController.Update(b.DeltaTime, Window.Size);

            OnUpdate(b.DeltaTime, b.TotalTime);
        };

        Window.Render += (a, b) =>
        {
            ImGuiHelpers.LeftTopOverlay("Overlay", () =>
            {
                ImGui.Text($"Backend: {Context.Backend}");

                ImGui.Separator();

                ImGui.Text(Context.Capabilities.DeviceName);

                ImGui.Separator();

                ImGui.Text($"Frame Time: {b.DeltaTime * 1000:F2}ms");

                ImGui.Separator();

                ImGui.Text($"FPS: {fps:F0}");
            });

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

            if (avgFrameTimes.Count is 10000)
            {
                avgFrameTimes.RemoveRange(0, 1000);
            }

            avgFrameTimes.Add(b.DeltaTime);

            if (b.TotalTime - lastTotalTime > 1)
            {
                fps = 1 / avgFrameTimes.Average();
                lastTotalTime = b.TotalTime;
            }
        };

        Window.SizeChanged += (a, b) =>
        {
            SwapChain.Resize();

            OnSizeChanged(b.Value.X, b.Value.Y);
        };

        Window.Unloaded += (a, b) =>
        {
            OnDestroy();

            CommandProcessor.Dispose();
            ImGuiController.Dispose();
            SwapChain.Dispose();
            Context.Dispose();

            double avgFrameTime = avgFrameTimes.Average();

            Console.WriteLine($"Backend: {backend}");
            Console.WriteLine($"Average Frame Time: {avgFrameTime * 1000:F2}ms");
            Console.WriteLine($"Average FPS: {1 / avgFrameTime:F0}");
        };
    }

    public IWindow Window { get; }

    public uint Width => Window.Size.X;

    public uint Height => Window.Size.Y;

    public Camera Camera { get; }

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

    protected abstract void OnSizeChanged(uint width, uint height);

    protected abstract void OnDestroy();
}
