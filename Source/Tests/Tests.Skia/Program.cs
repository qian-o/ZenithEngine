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
            _grContext.PurgeUnusedResources(2000);

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

        Demo(model);

        _views =
        [
            new AnimationView(Path.Combine("Assets", "LottieFiles", "demo1.json"), _device, _imGuiController, _grContext),
            new AnimationView(Path.Combine("Assets", "LottieFiles", "demo2.json"), _device, _imGuiController, _grContext),
            new PlotView("OxyPlot View", _device, _imGuiController, _grContext){ ActualModel = model }
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

    private static void Demo(PlotModel model)
    {
        const string xKey = "X";
        const string yKey = "Y";
        const string cKey = "Color";
        const string legendKey = "Legend";

        Random random = new(10);

        Axis x = new LinearAxis()
        {
            Key = xKey,
            Position = AxisPosition.Bottom,
            Title = "X Axis",
            Minimum = 0,
            Maximum = 100,
            AxislineStyle = LineStyle.Solid,
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
            AxislineStyle = LineStyle.Solid,
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
            XAxisKey = xKey,
            YAxisKey = yKey,
            ColorAxisKey = cKey,
            LegendKey = legendKey,
            MarkerType = MarkerType.Circle,
            MarkerSize = 4
        };

        for (int i = 0; i < 2500; i++)
        {
            ScatterPoint point = new(((random.NextDouble() * 2.2) - 1) * 200, (random.NextDouble() * 400) - 200)
            {
                Value = (random.NextDouble() * 200) - 100
            };

            scatterSeries.Points.Add(point);
        }

        model.Series.Add(scatterSeries);

        LineSeries lineSeries = new()
        {
            Title = "LineSeries",
            XAxisKey = xKey,
            YAxisKey = yKey,
            LegendKey = legendKey,
            Color = OxyColors.SkyBlue,
            StrokeThickness = 3,
            LineStyle = LineStyle.Dash,
            MarkerType = MarkerType.Circle,
            MarkerSize = 5,
            MarkerStroke = OxyColors.White,
            MarkerFill = OxyColors.SkyBlue,
            MarkerStrokeThickness = 1.5
        };

        int pointY = random.Next(10, 90);

        for (int pointX = 0; pointX < 100; pointX += 10)
        {
            lineSeries.Points.Add(new DataPoint(pointX, pointY));

            pointY += random.Next(-20, 20);
        }

        model.Series.Add(lineSeries);

        model.InvalidatePlot(true);
    }
}
