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
        this.length = length;

        BufferDesc desc = new((uint)(sizeof(T) * length), usage | BufferUsage.Dynamic, (uint)sizeof(T));

        buffer = context.Factory.CreateBuffer(in desc);

        mapped = context.MapMemory(buffer, MapMode.ReadWrite);
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= length)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range for buffer of length {length}.");
            }

            return new ReadOnlySpan<T>((void*)mapped.Data, (int)length)[index];
        }
        set
        {
            if (index < 0 || index >= length)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range for buffer of length {length}.");
            }

            Span<T> span = new((void*)mapped.Data, (int)length);

            span[index] = value;
        }
    }

    public void CopyFrom(ReadOnlySpan<T> source, uint offset = 0)
    {
        if (source.Length + offset > length)
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
