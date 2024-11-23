﻿using System.Runtime.InteropServices;
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
        T* ptr = (T*)Alloc((uint)(sizeof(T) * count));

        ptr[0] = default;

        return ptr;
    }

    public T* Alloc<T>(T[] values) where T : unmanaged
    {
        T* ptr = Alloc<T>(values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            ptr[i] = values[i];
        }

        return ptr;
    }

    public byte* AllocAnsi(string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);

        byte* chars = Alloc<byte>(bytes.Length + 1);

        Marshal.Copy(bytes, 0, (nint)chars, bytes.Length);

        return chars;
    }

    public byte** AllocAnsi(string[] values)
    {
        nint* ptr = Alloc<nint>(values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            ptr[i] = (nint)AllocAnsi(values[i]);
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
