using Graphics.Core;

namespace Graphics.Engine;

public abstract class GraphicsResource(GraphicsContext context) : DisposableObject
{
    private string name = string.Empty;

    public GraphicsContext Context { get; } = context;

    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            if (name == value)
            {
                return;
            }

            name = value;

            SetName(value);
        }
    }

    protected abstract void SetName(string name);
}
