using Graphics.Core;
using Graphics.Vulkan;

namespace Renderer.Components;

internal abstract class Control : DisposableObject
{
    protected readonly ImGuiController _imGuiController;

    public Control(MainWindow mainWindow)
    {
        _imGuiController = mainWindow.ImGuiController;

        Initialize();
    }

    public bool IsVisible { get; set; } = true;

    public void Update(UpdateEventArgs e)
    {
        if (IsVisible)
        {
            UpdateCore(e);
        }
    }

    public void Render(RenderEventArgs e)
    {
        if (IsVisible)
        {
            RenderCore(e);
        }
    }

    protected abstract void Initialize();

    protected abstract void UpdateCore(UpdateEventArgs e);

    protected abstract void RenderCore(RenderEventArgs e);

    protected override void Destroy()
    {
    }
}
