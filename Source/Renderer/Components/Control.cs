using Graphics.Core;

namespace Renderer.Components;

internal abstract class Control : MVVM
{
    public Control()
    {
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
