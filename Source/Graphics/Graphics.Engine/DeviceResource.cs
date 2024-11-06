using Graphics.Core;
using Graphics.Core.Helpers;

namespace Graphics.Engine;

public abstract class DeviceResource(Context context) : DisposableObject
{
    private string name = string.Empty;

    internal Allocator Allocator { get; } = new();

    public Context Context { get; } = context;

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

    protected override void Destroy()
    {
        Allocator.Dispose();
    }
}
