using System.Runtime.InteropServices;

namespace ZenithEngine.Common;

/// <summary>
/// Provides a persistent memory allocator.
/// </summary>
public unsafe class Allocator : DisposableObject
{
    private readonly Lock @lock = new();
    private readonly List<nint> blocks = [];

    public void* Alloc(uint size)
    {
        @lock.Enter();

        void* ptr = NativeMemory.Alloc(size);

        blocks.Add((nint)ptr);

        @lock.Exit();

        return ptr;
    }

    public T* Alloc<T>() where T : unmanaged
    {
        return (T*)Alloc((uint)sizeof(T));
    }

    public T* Alloc<T>(int count) where T : unmanaged
    {
        return (T*)Alloc((uint)sizeof(T) * count);
    }

    public T* Alloc<T>(params T[] values) where T : unmanaged
    {
        T* ptr = Alloc<T>(values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            ptr[i] = values[i];
        }

        return ptr;
    }

    public char** Alloc(params string[] values)
    {
        nint* ptr = Alloc<nint>(values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            char* str = Alloc<char>(values[i].Length + 1);

            for (int j = 0; j < values[i].Length; j++)
            {
                str[j] = values[i][j];
            }

            str[values[i].Length] = '\0';

            ptr[i] = (nint)str;
        }

        return (char**)ptr;
    }

    public void Free(void* ptr)
    {
        blocks.Remove((nint)ptr);

        NativeMemory.Free(ptr);
    }

    public void Free<T>(T* ptr) where T : unmanaged
    {
        Free((void*)ptr);
    }

    public void Free<T>(T** ptr, int count) where T : unmanaged
    {
        for (int i = 0; i < count; i++)
        {
            Free(ptr[i]);
        }

        Free(ptr);
    }

    public void Release()
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            NativeMemory.Free((void*)blocks[i]);
        }

        blocks.Clear();
    }

    protected override void Destroy()
    {
        Release();
    }
}
