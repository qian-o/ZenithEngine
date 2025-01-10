using System.Runtime.InteropServices;
using System.Text;

namespace ZenithEngine.Common;

/// <summary>
/// Provides a persistent memory allocator.
/// </summary>
public unsafe class MemoryAllocator : DisposableObject
{
    private readonly List<nint> blocks = [];

    public void* Alloc(uint size)
    {
        void* ptr = NativeMemory.Alloc(size);

        blocks.Add((nint)ptr);

        return ptr;
    }

    public T* Alloc<T>(uint count = 1) where T : unmanaged
    {
        T* ptr = (T*)Alloc((uint)(sizeof(T) * count));

        for (uint i = 0; i < count; i++)
        {
            ptr[i] = default;
        }

        return ptr;
    }

    public T* Alloc<T>(T[] values) where T : unmanaged
    {
        T* ptr = Alloc<T>((uint)values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            ptr[i] = values[i];
        }

        return ptr;
    }

    public byte* AllocUTF8(string value)
    {
        byte* ptr = Alloc<byte>(Utils.CalcSizeStringUTF8(value));

        byte[] bytes = Encoding.UTF8.GetBytes(value);

        Marshal.Copy(bytes, 0, (nint)ptr, bytes.Length);

        return ptr;
    }

    public byte** AllocUTF8(string[] values)
    {
        nint* ptr = Alloc<nint>((uint)values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            ptr[i] = (nint)AllocUTF8(values[i]);
        }

        return (byte**)ptr;
    }

    public byte** AllocUni(string value)
    {
        byte* ptr = Alloc<byte>(Utils.CalcSizeByStringUni(value));

        byte[] bytes = Encoding.Unicode.GetBytes(value);

        Marshal.Copy(bytes, 0, (nint)ptr, bytes.Length);

        return (byte**)ptr;
    }

    public byte** AllocUni(string[] values)
    {
        nint* ptr = Alloc<nint>((uint)values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            ptr[i] = (nint)AllocUni(values[i]);
        }

        return (byte**)ptr;
    }

    public void Free(void* ptr)
    {
        blocks.Remove((nint)ptr);

        NativeMemory.Free(ptr);
    }

    public void Free(byte** ptr, int count)
    {
        for (uint i = 0; i < count; i++)
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
