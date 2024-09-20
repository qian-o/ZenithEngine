using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;
using OxyPlot;
using OxyPlot.SkiaSharp;
using SkiaSharp;
using Tests.Core;

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

    private readonly ViewController _viewController = new();
    private readonly SkiaRenderContext _renderContext = new();
    private readonly PlotData _plotData = new();

    private PlotModel? model;

    public PlotView(GraphicsDevice device,
                    ImGuiController imGuiController,
                    GRContext grContext) : base("Plot View", device, imGuiController, grContext)
    {
        _viewController = new ViewController();
        _renderContext = new SkiaRenderContext();
        _plotData = new PlotData();

        _viewController.AddMouseDown(MouseDown);
        _viewController.AddMouseUp(MouseUp);
        _viewController.AddMouseMove(MouseMove);
        _viewController.AddMouseWheel(MouseWheel);
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
        _viewController.Update(Position);

        if (_plotData.ClipboardText != null)
        {
            ImGui.SetClipboardText(_plotData.ClipboardText);
        }

        if (_plotData.CursorType != null)
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

        ((IPlotModel)model).Render(_renderContext, ClientArea = new OxyRect(0, 0, Width, Height));
    }

    protected override void Destroy()
    {
        _renderContext.Dispose();

        base.Destroy();
    }

    private void MouseDown(ImGuiMouseButton mouseButton, Vector2 mousePosition)
    {
        OxyMouseButton oxyMouseButton = mouseButton switch
        {
            ImGuiMouseButton.Left => OxyMouseButton.Left,
            ImGuiMouseButton.Right => OxyMouseButton.Right,
            ImGuiMouseButton.Middle => OxyMouseButton.Middle,
            _ => OxyMouseButton.None
        };

        ActualController.HandleMouseDown(this, new OxyMouseDownEventArgs()
        {
            ChangedButton = oxyMouseButton,
            Position = new ScreenPoint(mousePosition.X, mousePosition.Y),
            ModifierKeys = OxyModifierKeys.None,
            ClickCount = 1
        });
    }

    private void MouseUp(ImGuiMouseButton mouseButton, Vector2 mousePosition)
    {
        ActualController.HandleMouseUp(this, new OxyMouseEventArgs()
        {
            Position = new ScreenPoint(mousePosition.X, mousePosition.Y),
            ModifierKeys = OxyModifierKeys.None
        });
    }

    private void MouseMove(Vector2 mousePosition)
    {
        ActualController.HandleMouseMove(this, new OxyMouseEventArgs()
        {
            Position = new ScreenPoint(mousePosition.X, mousePosition.Y),
            ModifierKeys = OxyModifierKeys.None
        });
    }

    private void MouseWheel(Vector2 mousePosition, float delta)
    {
        ActualController.HandleMouseWheel(this, new OxyMouseWheelEventArgs()
        {
            Position = new ScreenPoint(mousePosition.X, mousePosition.Y),
            Delta = Convert.ToInt32(delta * 120),
            ModifierKeys = OxyModifierKeys.None
        });
    }
}
