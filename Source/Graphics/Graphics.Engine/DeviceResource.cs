using Graphics.Core.Helpers;

namespace Graphics.Engine;

public abstract class DeviceResource(Context context) : IDisposable
{
    private volatile uint isDisposed;

    private string name = string.Empty;

    protected Allocator Allocator { get; } = new();

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

    public bool IsDisposed => isDisposed != 0;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref isDisposed, 1) != 0)
        {
            return;
        }

        Destroy();

        Allocator.Dispose();

        GC.SuppressFinalize(this);
    }

    protected abstract void SetName(string name);

    protected abstract void Destroy();
}
