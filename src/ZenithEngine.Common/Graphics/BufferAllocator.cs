using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public class BufferAllocator(GraphicsContext context) : GraphicsResource(context)
{
    private const uint MinBufferSize = 1024 * 4;
    private const uint MaxBufferCount = 100;

    private readonly List<Buffer> available = [];
    private readonly List<Buffer> inUse = [];

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

            buffer = Context.Factory.CreateBuffer(in desc);
        }

        inUse.Add(buffer);

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

        available.AddRange(inUse);
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        foreach (Buffer item in available)
        {
            item.Dispose();
        }

        foreach (Buffer item in inUse)
        {
            item.Dispose();
        }

        available.Clear();
        inUse.Clear();
    }
}
