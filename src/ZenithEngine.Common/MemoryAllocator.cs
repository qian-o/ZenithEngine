using System.Runtime.InteropServices;
using System.Text;

namespace ZenithEngine.Common;

/// <summary>
/// Provides a persistent memory allocator.
/// </summary>
public unsafe class MemoryAllocator : DisposableObject
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

    public T* Alloc<T>(int count = 1) where T : unmanaged
    {
        return (T*)Alloc((uint)(sizeof(T) * count));
    }

    public char* AllocAnsi(string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);

        char* chars = Alloc<char>(bytes.Length + 1);
        Marshal.Copy(bytes, 0, (nint)chars, bytes.Length);
        chars[bytes.Length] = '\0';

        return chars;
    }

    public char** AllocAnsi(string[] values)
    {
        nint* ptr = Alloc<nint>(values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            ptr[i] = (nint)AllocAnsi(values[i]);
        }

        return (char**)ptr;
    }

    public void Free(void* ptr)
    {
        blocks.Remove((nint)ptr);

        NativeMemory.Free(ptr);
    }

    public void Free(char** ptr, int count)
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
