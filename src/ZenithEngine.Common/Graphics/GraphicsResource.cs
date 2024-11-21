﻿namespace ZenithEngine.Common.Graphics;

public abstract class GraphicsResource(GraphicsContext context) : IDisposable
{
    private volatile uint isDisposed;

    private string name = string.Empty;

    public string Name
    {
        get => name;
        set
        {
            if (name != value)
            {
                name = value;

                DebugName(value);
            }
        }
    }

    public bool IsDisposed => isDisposed != 0;

    /// <summary>
    /// Graphics context.
    /// </summary>
    protected GraphicsContext Context { get; } = context;

    /// <summary>
    /// Current resource lifecycle persistent memory allocator.
    /// </summary>
    protected MemoryAllocator MemoryAllocator { get; } = new();

    /// <summary>
    /// Command recording period available temporary buffer allocator.
    /// </summary>
    protected BufferAllocator BufferAllocator => Context.BufferAllocator!;

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

    protected abstract void DebugName(string name);

    protected abstract void Destroy();
}
