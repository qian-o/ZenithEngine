using Graphics.Core;
using Graphics.Core.Helpers;

namespace Graphics.Engine;

public abstract class DeviceResource(Context context) : DisposableObject
{
    private string name = string.Empty;

    public Context Context { get; } = context;

    public Allocator Allocator { get; } = new();

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

    override protected void Destroy()
    {
        Allocator.Dispose();
    }
}
