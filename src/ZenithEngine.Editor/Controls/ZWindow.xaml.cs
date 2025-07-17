using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Graphics;
using ZenithEngine.Editor.Helpers;

namespace ZenithEngine.Editor.Controls;

public sealed partial class ZWindow : Window
{
    private readonly OverlappedPresenter presenter;

    private PointInt32? start;

    public ZWindow(UIElement content, bool firstMaximize)
    {
        InitializeComponent();

        ContentBorder.Child = content;

        ExtendsContentIntoTitleBar = true;

        AppWindow.SetPresenter(presenter = OverlappedPresenter.Create());

        presenter.SetBorderAndTitleBar(false, false);

        if (firstMaximize)
        {
            bool isfirst = false;

            Activated += (_, _) =>
            {
                if (isfirst)
                {
                    return;
                }

                presenter.Maximize();

                isfirst = true;
            };
        }
    }

    private void DragArea_DragDelta(object sender, DragDeltaEventArgs e)
    {
        PointInt32 current = PointerHelpers.GetPosition();

        if (start is null)
        {
            if (presenter.State is OverlappedPresenterState.Maximized)
            {
                presenter.Restore();

                AppWindow.Move(current);
            }

            start = new()
            {
                X = current.X - AppWindow.Position.X,
                Y = current.Y - AppWindow.Position.Y
            };
        }
        else
        {
            AppWindow.Move(new()
            {
                X = current.X - start.Value.X,
                Y = current.Y - start.Value.Y
            });
        }
    }

    private void DragArea_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        start = null;
    }
}
