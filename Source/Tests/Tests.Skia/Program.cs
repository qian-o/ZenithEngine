using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using SkiaSharp;
using Tests.Core;

namespace Tests.Skia;

internal sealed unsafe class Program
{
    private static GraphicsDevice _device = null!;
    private static ImGuiController _imGuiController = null!;
    private static GRContext _grContext = null!;
    private static View[] _views = null!;

    private static void Main(string[] _)
    {
        using Window window = Window.CreateWindowByVulkan();
        window.Title = "Tests.Skia";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice(), window);
        using ImGuiController imGuiController = new(window,
                                                    device,
                                                    new ImGuiFontConfig("Assets/Fonts/msyh.ttf", 16, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()),
                                                    ImGuiSizeConfig.Default);
        using GRContext grContext = SkiaGraphics.CreateContext(device);

        using CommandList commandList = device.Factory.CreateGraphicsCommandList();

        window.Load += Load;

        window.Update += (a, b) =>
        {
            imGuiController.Update(b.DeltaTime);

            Update(a, b);
        };

        window.Render += (a, b) =>
        {
            Render(a, b);

            _grContext.Flush(true);
            _grContext.PurgeUnusedResources(1000);

            commandList.Begin();
            {
                commandList.SetFramebuffer(device.MainSwapchain.Framebuffer);
                commandList.ClearColorTarget(0, RgbaFloat.Black);
                commandList.ClearDepthStencil(1.0f);

                imGuiController.Render(commandList);
            }
            commandList.End();

            device.SubmitCommandsAndSwapBuffers(commandList, device.MainSwapchain);

            imGuiController.PlatformSwapBuffers();
        };

        window.Resize += (a, b) =>
        {
            device.MainSwapchain.Resize(b.Width, b.Height);

            Resize(a, b);
        };

        window.Closing += Closing;

        _device = device;
        _imGuiController = imGuiController;
        _grContext = grContext;

        window.Run();
    }

    private static void Load(object? sender, LoadEventArgs e)
    {
        PlotModel model = new()
        {
            Title = "Simple Plot",
            Subtitle = "This is a simple plot"
        };

        FillPlotModel(model);

        _views =
        [
            new AnimationView(Path.Combine("Assets", "LottieFiles", "demo1.json"), _device, _imGuiController, _grContext),
            new AnimationView(Path.Combine("Assets", "LottieFiles", "demo2.json"), _device, _imGuiController, _grContext),
            new PlotView(_device, _imGuiController, _grContext){ ActualModel = model }
        ];
    }

    private static void Update(object? sender, UpdateEventArgs e)
    {
        foreach (View view in _views)
        {
            view.Update(e);
        }
    }

    private static void Render(object? sender, RenderEventArgs e)
    {
        foreach (View view in _views)
        {
            view.Render(e);
        }

        ImGui.Begin("Tests.Skia");
        {
            ImGui.Text($"FPS: {1.0f / e.DeltaTime}");

            ImGui.Separator();

            ImGui.Text($"Total Time: {e.TotalTime}");

            ImGui.Separator();

            ImGui.Text($"Delta Time: {e.DeltaTime}");

            ImGui.End();
        }
    }

    private static void Resize(object? sender, ResizeEventArgs e)
    {
    }

    private static void Closing(object? sender, ClosingEventArgs e)
    {
        foreach (View view in _views)
        {
            view.Dispose();
        }
    }

    private static void FillPlotModel(PlotModel model)
    {
        const string xKey = "X";
        const string yKey = "Y";
        const string cKey = "Color";
        const string legendKey = "Legend";

        Axis x = new LinearAxis()
        {
            Key = xKey,
            Position = AxisPosition.Bottom,
            Title = "X Axis",
            Minimum = 0,
            Maximum = 100,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot
        };

        model.Axes.Add(x);

        Axis y = new LinearAxis()
        {
            Key = yKey,
            Position = AxisPosition.Left,
            Title = "Y Axis",
            Minimum = 0,
            Maximum = 100,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot
        };

        model.Axes.Add(y);

        Axis c = new LinearColorAxis()
        {
            Key = cKey,
            Position = AxisPosition.Right,
            Title = "Color Axis",
            Minimum = 0,
            Maximum = 100,
            Palette = OxyPalettes.Cool(32),
            LowColor = OxyColors.Undefined,
            HighColor = OxyColors.Undefined
        };

        model.Axes.Add(c);

        Legend legend = new()
        {
            Key = legendKey,
            LegendPosition = LegendPosition.RightTop,
            LegendOrientation = LegendOrientation.Vertical,
            LegendBorder = OxyColors.Black,
            LegendBorderThickness = 1
        };

        model.Legends.Add(legend);

        ScatterSeries scatterSeries = new()
        {
            Title = "ScatterSeries",
            MarkerType = MarkerType.Circle,
            MarkerSize = 4,
            XAxisKey = xKey,
            YAxisKey = yKey,
            ColorAxisKey = cKey,
            LegendKey = legendKey
        };

        Random random = new();

        for (int i = 0; i < 2500; i++)
        {
            ScatterPoint point = new((random.NextDouble() * 2.2 - 1) * 200, random.NextDouble() * 200)
            {
                Value = random.NextDouble() * 100
            };

            scatterSeries.Points.Add(point);
        }

        model.Series.Add(scatterSeries);

        LineSeries lineSeries = new()
        {
            Title = "LineSeries",
            Color = OxyColors.Blue,
            XAxisKey = xKey,
            YAxisKey = yKey,
            LegendKey = legendKey
        };

        for (int i = 0; i < 100; i++)
        {
            lineSeries.Points.Add(new DataPoint(i, i));
        }

        model.Series.Add(lineSeries);

        model.InvalidatePlot(true);
    }
}
