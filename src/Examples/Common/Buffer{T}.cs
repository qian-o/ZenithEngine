﻿using System.Runtime.CompilerServices;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using Buffer = ZenithEngine.Common.Graphics.Buffer;

namespace Common;

public unsafe class Buffer<T> : DisposableObject where T : unmanaged
{
    private readonly GraphicsContext context;
    private readonly uint length;
    private readonly Buffer buffer;
    private readonly MappedResource mapped;

    public Buffer(GraphicsContext context, uint length, BufferUsage usage)
    {
        this.context = context;
        this.length = length is 0 ? 1 : length;

        BufferDesc desc = new((uint)(sizeof(T) * length), usage | BufferUsage.Dynamic, (uint)sizeof(T));

        buffer = context.Factory.CreateBuffer(in desc);

        mapped = context.MapMemory(buffer, MapMode.ReadWrite);
    }

    public ref T this[int index]
    {
        get
        {
            if (index < 0 || index >= length)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range for buffer of length {length}.");
            }

            return ref Unsafe.AsRef<T>((void*)(mapped.Data + (index * sizeof(T))));
        }
    }

    public static implicit operator Buffer(Buffer<T> buffer)
    {
        return buffer.buffer;
    }

    public void CopyFrom(ReadOnlySpan<T> source)
    {
        if (source.Length > length)
        {
            throw new ArgumentOutOfRangeException(nameof(source), "Source span exceeds buffer length.");
        }

        Span<T> span = new((void*)mapped.Data, (int)length);

        source.CopyTo(span);
    }

    protected override void Destroy()
    {
        context.UnmapMemory(buffer);

        buffer.Dispose();
    }
}
