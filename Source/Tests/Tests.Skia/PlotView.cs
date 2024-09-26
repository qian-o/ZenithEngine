using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using OxyPlot;
using OxyPlot.SkiaSharp;
using SkiaSharp;
using Tests.Core;
using MouseButtonEventArgs = Tests.Core.MouseButtonEventArgs;
using MouseMoveEventArgs = Tests.Core.MouseMoveEventArgs;
using MouseWheelEventArgs = Tests.Core.MouseWheelEventArgs;

namespace Tests.Skia;

internal sealed class PlotView : SkiaView, IPlotView
{
    private sealed class PlotData
    {
        public TrackerHitResult? TrackerHitResult { get; set; }

        public OxyRect? ZoomRectangle { get; set; }

        public string? ClipboardText { get; set; }

        public CursorType? CursorType { get; set; }
    }

    private const int TrackerTextPadding = 5;

    private const string TrackerLineStroke = "#80000000";
    private const string TrackerBackground = "#E0FFFFA0";
    private const string ZoomRectangleFill = "#40FFFF00";

    private readonly ViewController _viewController;
    private readonly SkiaRenderContext _renderContext;
    private readonly PlotData _plotData;

    private PlotModel? model;

    public PlotView(GraphicsDevice device,
                    ImGuiController imGuiController,
                    GRContext grContext) : base("Plot View", device, imGuiController, grContext)
    {
        _viewController = new ViewController(this);
        _renderContext = new SkiaRenderContext();
        _plotData = new PlotData();

        _viewController.MouseDown += MouseDown;
        _viewController.MouseUp += MouseUp;
        _viewController.MouseMove += MouseMove;
        _viewController.MouseWheel += MouseWheel;
    }

    public PlotModel? ActualModel
    {
        get => model;
        set
        {
            if (model != value)
            {
                if (model != null)
                {
                    ((IPlotModel)model).AttachPlotView(null);
                }

                model = value;

                if (model != null)
                {
                    ((IPlotModel)model).AttachPlotView(this);

                    model.InvalidatePlot(true);
                }
            }
        }
    }

    public IController ActualController { get; set; } = new PlotController();

    public OxyRect ClientArea { get; private set; }

    Model? IView.ActualModel => model;

    public void ShowTracker(TrackerHitResult trackerHitResult)
    {
        _plotData.TrackerHitResult = trackerHitResult;
    }

    public void ShowZoomRectangle(OxyRect rectangle)
    {
        _plotData.ZoomRectangle = rectangle;
    }

    public void HideTracker()
    {
        _plotData.TrackerHitResult = null;
    }

    public void HideZoomRectangle()
    {
        _plotData.ZoomRectangle = null;
    }

    public void SetClipboardText(string text)
    {
        _plotData.ClipboardText = text;
    }

    public void SetCursorType(CursorType cursorType)
    {
        _plotData.CursorType = cursorType;
    }

    public void InvalidatePlot(bool updateData = true)
    {
        if (model != null)
        {
            ((IPlotModel)model).Update(updateData);
        }
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
        _viewController.Update();

        if (_plotData.ClipboardText != null)
        {
            ImGui.SetClipboardText(_plotData.ClipboardText);
        }

        if (_plotData.CursorType is not null and not CursorType.Default)
        {
            ImGuiMouseCursor cursorType = _plotData.CursorType.Value switch
            {
                CursorType.Pan => ImGuiMouseCursor.Hand,
                CursorType.ZoomHorizontal => ImGuiMouseCursor.ResizeEw,
                CursorType.ZoomVertical => ImGuiMouseCursor.ResizeNs,
                CursorType.ZoomRectangle => ImGuiMouseCursor.ResizeAll,
                _ => ImGuiMouseCursor.Arrow
            };

            ImGui.SetMouseCursor(cursorType);
        }
    }

    protected override void OnRenderSurface(SKCanvas canvas, RenderEventArgs e)
    {
        if (model == null)
        {
            return;
        }

        _renderContext.SkCanvas = canvas;

        ((IPlotModel)model).Render(_renderContext, ClientArea = new OxyRect(0, 0, ActualWidth, ActualHeight));

        if (_plotData.TrackerHitResult != null)
        {
            _renderContext.PushClip(model.PlotArea);
            {
                ScreenPoint beginH = new((float)model.PlotArea.Left, (float)_plotData.TrackerHitResult.Position.Y);
                ScreenPoint endH = new((float)model.PlotArea.Right, (float)_plotData.TrackerHitResult.Position.Y);

                _renderContext.DrawLine([beginH, endH], OxyColor.Parse(TrackerLineStroke), 1, EdgeRenderingMode.Automatic);

                ScreenPoint beginV = new((float)_plotData.TrackerHitResult.Position.X, (float)model.PlotArea.Top);
                ScreenPoint endV = new((float)_plotData.TrackerHitResult.Position.X, (float)model.PlotArea.Bottom);

                _renderContext.DrawLine([beginV, endV], OxyColor.Parse(TrackerLineStroke), 1, EdgeRenderingMode.Automatic);
            }
            _renderContext.PopClip();

            OxySize textSize = _renderContext.MeasureText(_plotData.TrackerHitResult.Text, model.DefaultFont, model.DefaultFontSize);
            textSize = new OxySize(textSize.Width + (TrackerTextPadding * 2), textSize.Height + (TrackerTextPadding * 2));

            double x = Math.Min(Math.Max(0, _plotData.TrackerHitResult.Position.X - (textSize.Width / 2)), ActualWidth - textSize.Width);
            double y = Math.Min(Math.Max(0, _plotData.TrackerHitResult.Position.Y - textSize.Height - 7), ActualHeight - textSize.Height);

            _renderContext.DrawRectangle(new OxyRect(x, y, textSize.Width, textSize.Height),
                                         OxyColor.Parse(TrackerBackground),
                                         OxyColors.Black,
                                         1,
                                         EdgeRenderingMode.Automatic);

            _renderContext.DrawText(new ScreenPoint((float)x + TrackerTextPadding, (float)y + TrackerTextPadding),
                                    _plotData.TrackerHitResult.Text,
                                    OxyColors.Black,
                                    model.DefaultFont,
                                    model.DefaultFontSize);
        }

        if (_plotData.ZoomRectangle != null)
        {
            _renderContext.PushClip(model.PlotArea);
            {
                ScreenPoint leftTop = new((float)_plotData.ZoomRectangle.Value.Left, (float)_plotData.ZoomRectangle.Value.Top);
                ScreenPoint rightTop = new((float)_plotData.ZoomRectangle.Value.Right, (float)_plotData.ZoomRectangle.Value.Top);
                ScreenPoint rightBottom = new((float)_plotData.ZoomRectangle.Value.Right, (float)_plotData.ZoomRectangle.Value.Bottom);
                ScreenPoint leftBottom = new((float)_plotData.ZoomRectangle.Value.Left, (float)_plotData.ZoomRectangle.Value.Bottom);

                _renderContext.DrawLine([leftTop, rightTop, rightBottom, leftBottom, leftTop], OxyColors.Black, 1, EdgeRenderingMode.Automatic, [3, 1]);
                _renderContext.FillRectangle(_plotData.ZoomRectangle.Value, OxyColor.Parse(ZoomRectangleFill), EdgeRenderingMode.Automatic);
            }
            _renderContext.PopClip();
        }
    }

    protected override void Destroy()
    {
        _renderContext.Dispose();

        base.Destroy();
    }

    private void MouseDown(object? sender, MouseButtonEventArgs e)
    {
        OxyMouseButton oxyMouseButton = e.Button switch
        {
            ImGuiMouseButton.Left => OxyMouseButton.Left,
            ImGuiMouseButton.Right => OxyMouseButton.Right,
            ImGuiMouseButton.Middle => OxyMouseButton.Middle,
            _ => OxyMouseButton.None
        };

        ActualController.HandleMouseDown(this, new OxyMouseDownEventArgs()
        {
            ChangedButton = oxyMouseButton,
            Position = new ScreenPoint(e.Position.X, e.Position.Y),
            ModifierKeys = GetModifierKeys(),
            ClickCount = 1
        });
    }

    private void MouseUp(object? sender, MouseButtonEventArgs e)
    {
        ActualController.HandleMouseUp(this, new OxyMouseEventArgs()
        {
            Position = new ScreenPoint(e.Position.X, e.Position.Y),
            ModifierKeys = GetModifierKeys()
        });
    }

    private void MouseMove(object? sender, MouseMoveEventArgs e)
    {
        ActualController.HandleMouseMove(this, new OxyMouseEventArgs()
        {
            Position = new ScreenPoint(e.Position.X, e.Position.Y),
            ModifierKeys = GetModifierKeys()
        });
    }

    private void MouseWheel(object? sender, MouseWheelEventArgs e)
    {
        ActualController.HandleMouseWheel(this, new OxyMouseWheelEventArgs()
        {
            Position = new ScreenPoint(e.Position.X, e.Position.Y),
            Delta = Convert.ToInt32(e.Wheel * 120),
            ModifierKeys = GetModifierKeys()
        });
    }

    private static OxyModifierKeys GetModifierKeys()
    {
        OxyModifierKeys modifierKeys = OxyModifierKeys.None;

        if (ImGui.IsKeyDown(ImGuiKey.ModShift))
        {
            modifierKeys |= OxyModifierKeys.Shift;
        }

        if (ImGui.IsKeyDown(ImGuiKey.ModCtrl))
        {
            modifierKeys |= OxyModifierKeys.Control;
        }

        if (ImGui.IsKeyDown(ImGuiKey.ModAlt))
        {
            modifierKeys |= OxyModifierKeys.Alt;
        }

        if (ImGui.IsKeyDown(ImGuiKey.ModSuper))
        {
            modifierKeys |= OxyModifierKeys.Windows;
        }

        return modifierKeys;
    }
}
