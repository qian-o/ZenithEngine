using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public class BufferAllocator(GraphicsContext context) : DisposableObject
{
    private const uint MinBufferSize = 1024 * 4;
    private const uint MaxBufferCount = 100;

    private readonly Lock @lock = new();
    private readonly List<Buffer> availableBuffers = [];
    private readonly List<Buffer> usedBuffers = [];

    public Buffer Buffer(uint sizeInBytes)
    {
        @lock.Enter();

        Buffer? buffer = null;

        foreach (Buffer item in availableBuffers)
        {
            if (item.Desc.SizeInBytes >= sizeInBytes)
            {
                buffer = item;

                availableBuffers.Remove(item);

                break;
            }
        }

        if (buffer is null)
        {
            sizeInBytes = Math.Max(sizeInBytes, MinBufferSize);

            BufferDesc desc = BufferDesc.Default(sizeInBytes);

            buffer = context.Factory.CreateBuffer(in desc);
        }

        usedBuffers.Add(buffer);

        @lock.Exit();

        return buffer;
    }

    public void Release()
    {
        @lock.Enter();

        if (availableBuffers.Count > MaxBufferCount)
        {
            foreach (Buffer item in availableBuffers)
            {
                item.Dispose();
            }

            availableBuffers.Clear();
        }

        foreach (Buffer item in usedBuffers)
        {
            availableBuffers.Add(item);
        }

        @lock.Exit();
    }

    protected override void Destroy()
    {
        foreach (Buffer item in availableBuffers)
        {
            item.Dispose();
        }

        availableBuffers.Clear();

        foreach (Buffer item in usedBuffers)
        {
            item.Dispose();
        }

        usedBuffers.Clear();
    }
}
