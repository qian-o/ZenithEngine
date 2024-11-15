namespace ZenithEngine.Common;

public abstract class GraphicsResource(GraphicsContext context) : IDisposable
{
    private volatile uint isDisposed;

    private string name = string.Empty;

    public string Name
    {
        get => name;
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

    protected GraphicsContext Context { get; } = context;

    /// <summary>
    /// Persistent memory allocator.
    /// </summary>
    protected MemoryAllocator MemoryAllocator { get; } = new();

    public void Dispose()
    {
        if (Interlocked.Exchange(ref isDisposed, 1) != 0)
        {
            return;
        }

        Destroy();

        MemoryAllocator.Dispose();

        GC.SuppressFinalize(this);
    }

    protected abstract void SetName(string name);

    protected abstract void Destroy();
}
