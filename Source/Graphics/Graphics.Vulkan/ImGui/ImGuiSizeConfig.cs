using System.Numerics;
using Hexa.NET.ImGui;

namespace Graphics.Vulkan;

public readonly record struct ImGuiSizeConfig
{
    public ImGuiSizeConfig(Vector2 windowPadding,
                           float windowRounding,
                           Vector2 windowMinSize,
                           float childRounding,
                           float popupRounding,
                           Vector2 framePadding,
                           float frameRounding,
                           Vector2 itemSpacing,
                           Vector2 itemInnerSpacing,
                           Vector2 cellPadding,
                           Vector2 touchExtraPadding,
                           float indentSpacing,
                           float columnsMinSpacing,
                           float scrollbarSize,
                           float scrollbarRounding,
                           float grabMinSize,
                           float grabRounding,
                           float logSliderDeadzone,
                           float tabRounding,
                           float tabMinWidthForCloseButton,
                           Vector2 separatorTextPadding,
                           Vector2 displayWindowPadding,
                           Vector2 displaySafeAreaPadding,
                           float mouseCursorScale)
    {
        WindowPadding = windowPadding;
        WindowRounding = windowRounding;
        WindowMinSize = windowMinSize;
        ChildRounding = childRounding;
        PopupRounding = popupRounding;
        FramePadding = framePadding;
        FrameRounding = frameRounding;
        ItemSpacing = itemSpacing;
        ItemInnerSpacing = itemInnerSpacing;
        CellPadding = cellPadding;
        TouchExtraPadding = touchExtraPadding;
        IndentSpacing = indentSpacing;
        ColumnsMinSpacing = columnsMinSpacing;
        ScrollbarSize = scrollbarSize;
        ScrollbarRounding = scrollbarRounding;
        GrabMinSize = grabMinSize;
        GrabRounding = grabRounding;
        LogSliderDeadzone = logSliderDeadzone;
        TabRounding = tabRounding;
        TabMinWidthForCloseButton = tabMinWidthForCloseButton;
        SeparatorTextPadding = separatorTextPadding;
        DisplayWindowPadding = displayWindowPadding;
        DisplaySafeAreaPadding = displaySafeAreaPadding;
        MouseCursorScale = mouseCursorScale;
    }

    public Vector2 WindowPadding { get; init; }

    public float WindowRounding { get; init; }

    public Vector2 WindowMinSize { get; init; }

    public float ChildRounding { get; init; }

    public float PopupRounding { get; init; }

    public Vector2 FramePadding { get; init; }

    public float FrameRounding { get; init; }

    public Vector2 ItemSpacing { get; init; }

    public Vector2 ItemInnerSpacing { get; init; }

    public Vector2 CellPadding { get; init; }

    public Vector2 TouchExtraPadding { get; init; }

    public float IndentSpacing { get; init; }

    public float ColumnsMinSpacing { get; init; }

    public float ScrollbarSize { get; init; }

    public float ScrollbarRounding { get; init; }

    public float GrabMinSize { get; init; }

    public float GrabRounding { get; init; }

    public float LogSliderDeadzone { get; init; }

    public float TabRounding { get; init; }

    public float TabMinWidthForCloseButton { get; init; }

    public Vector2 SeparatorTextPadding { get; init; }

    public Vector2 DisplayWindowPadding { get; init; }

    public Vector2 DisplaySafeAreaPadding { get; init; }

    public float MouseCursorScale { get; init; }

    public static ImGuiSizeConfig Default => new()
    {
        WindowPadding = new Vector2(8, 8),
        WindowRounding = 0.0f,
        WindowMinSize = new Vector2(32, 32),
        ChildRounding = 0.0f,
        PopupRounding = 0.0f,
        FramePadding = new Vector2(4, 3),
        FrameRounding = 0.0f,
        ItemSpacing = new Vector2(8, 4),
        ItemInnerSpacing = new Vector2(4, 4),
        CellPadding = new Vector2(4, 2),
        TouchExtraPadding = new Vector2(0, 0),
        IndentSpacing = 21.0f,
        ColumnsMinSpacing = 6.0f,
        ScrollbarSize = 14.0f,
        ScrollbarRounding = 9.0f,
        GrabMinSize = 12.0f,
        GrabRounding = 0.0f,
        LogSliderDeadzone = 4.0f,
        TabRounding = 4.0f,
        TabMinWidthForCloseButton = 0.0f,
        SeparatorTextPadding = new Vector2(20, 3),
        DisplayWindowPadding = new Vector2(19, 19),
        DisplaySafeAreaPadding = new Vector2(3, 3),
        MouseCursorScale = 1.0f
    };

    public readonly ImGuiSizeConfig Scale(float scale)
    {
        ImGuiSizeConfig imGuiSizeConfig = new()
        {
            WindowPadding = ScaleVector2(WindowPadding, scale),
            WindowRounding = ScaleFloat(WindowRounding, scale),
            WindowMinSize = ScaleVector2(WindowMinSize, scale),
            ChildRounding = ScaleFloat(ChildRounding, scale),
            PopupRounding = ScaleFloat(PopupRounding, scale),
            FramePadding = ScaleVector2(FramePadding, scale),
            FrameRounding = ScaleFloat(FrameRounding, scale),
            ItemSpacing = ScaleVector2(ItemSpacing, scale),
            ItemInnerSpacing = ScaleVector2(ItemInnerSpacing, scale),
            CellPadding = ScaleVector2(CellPadding, scale),
            TouchExtraPadding = ScaleVector2(TouchExtraPadding, scale),
            IndentSpacing = ScaleFloat(IndentSpacing, scale),
            ColumnsMinSpacing = ScaleFloat(ColumnsMinSpacing, scale),
            ScrollbarSize = ScaleFloat(ScrollbarSize, scale),
            ScrollbarRounding = ScaleFloat(ScrollbarRounding, scale),
            GrabMinSize = ScaleFloat(GrabMinSize, scale),
            GrabRounding = ScaleFloat(GrabRounding, scale),
            LogSliderDeadzone = ScaleFloat(LogSliderDeadzone, scale),
            TabRounding = ScaleFloat(TabRounding, scale),
            TabMinWidthForCloseButton = ScaleFloat(TabMinWidthForCloseButton, scale),
            SeparatorTextPadding = ScaleVector2(SeparatorTextPadding, scale),
            DisplayWindowPadding = ScaleVector2(DisplayWindowPadding, scale),
            DisplaySafeAreaPadding = ScaleVector2(DisplaySafeAreaPadding, scale),
            MouseCursorScale = ScaleFloat(MouseCursorScale, scale)
        };

        return imGuiSizeConfig;
    }

    public readonly void Apply(ImGuiStylePtr style)
    {
        style.WindowPadding = WindowPadding;
        style.WindowRounding = WindowRounding;
        style.WindowMinSize = WindowMinSize;
        style.ChildRounding = ChildRounding;
        style.PopupRounding = PopupRounding;
        style.FramePadding = FramePadding;
        style.FrameRounding = FrameRounding;
        style.ItemSpacing = ItemSpacing;
        style.ItemInnerSpacing = ItemInnerSpacing;
        style.CellPadding = CellPadding;
        style.TouchExtraPadding = TouchExtraPadding;
        style.IndentSpacing = IndentSpacing;
        style.ColumnsMinSpacing = ColumnsMinSpacing;
        style.ScrollbarSize = ScrollbarSize;
        style.ScrollbarRounding = ScrollbarRounding;
        style.GrabMinSize = GrabMinSize;
        style.GrabRounding = GrabRounding;
        style.LogSliderDeadzone = LogSliderDeadzone;
        style.TabRounding = TabRounding;
        style.TabMinWidthForCloseButton = TabMinWidthForCloseButton;
        style.SeparatorTextPadding = SeparatorTextPadding;
        style.DisplayWindowPadding = DisplayWindowPadding;
        style.DisplaySafeAreaPadding = DisplaySafeAreaPadding;
        style.MouseCursorScale = MouseCursorScale;
    }

    private static float ScaleFloat(float value, float scale) => Convert.ToInt32(value * scale);

    private static Vector2 ScaleVector2(Vector2 value, float scale) => new(Convert.ToInt32(value.X * scale), Convert.ToInt32(value.Y * scale));
}
