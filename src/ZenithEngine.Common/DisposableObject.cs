namespace ZenithEngine.Common;

public abstract class DisposableObject : IDisposable
{
    private volatile uint isDisposed;

    ~DisposableObject()
    {
        Dispose();
    }

    public bool IsDisposed => isDisposed != 0;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref isDisposed, 1) != 0)
        {
            return;
        }

        Destroy();

        GC.SuppressFinalize(this);
    }

    protected abstract void Destroy();
}
