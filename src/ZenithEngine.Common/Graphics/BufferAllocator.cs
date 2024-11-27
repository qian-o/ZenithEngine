using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public class BufferAllocator(GraphicsContext context) : DisposableObject
{
    private const uint MinBufferSize = 1024 * 4;
    private const uint MaxBufferCount = 100;

    private readonly List<Buffer> available = [];
    private readonly List<Buffer> used = [];

    public Buffer Buffer(uint sizeInBytes)
    {
        Buffer? buffer = null;

        foreach (Buffer item in available)
        {
            if (item.Desc.SizeInBytes >= sizeInBytes)
            {
                buffer = item;

                available.Remove(item);

                break;
            }
        }

        if (buffer is null)
        {
            sizeInBytes = Math.Max(sizeInBytes, MinBufferSize);

            BufferDesc desc = BufferDesc.Default(sizeInBytes);

            buffer = context.Factory.CreateBuffer(in desc);
        }

        used.Add(buffer);

        return buffer;
    }

    public void Release()
    {
        if (available.Count > MaxBufferCount)
        {
            foreach (Buffer item in available)
            {
                item.Dispose();
            }

            available.Clear();
        }

        foreach (Buffer item in used)
        {
            available.Add(item);
        }
    }

    protected override void Destroy()
    {
        foreach (Buffer item in available)
        {
            item.Dispose();
        }

        foreach (Buffer item in used)
        {
            item.Dispose();
        }

        available.Clear();
        used.Clear();
    }
}
