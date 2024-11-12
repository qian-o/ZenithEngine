using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

internal class BufferPool(Context context) : DeviceResource(context)
{
    private const uint MinBufferSize = 1024 * 4;
    private const uint MaxBufferCount = 100;

    private readonly List<Buffer> buffers = [];

    public Buffer Buffer(uint sizeInBytes)
    {
        lock (this)
        {
            foreach (var buffer in buffers)
            {
                if (buffer.Desc.SizeInBytes >= sizeInBytes)
                {
                    buffers.Remove(buffer);

                    return buffer;
                }
            }

            uint size = Math.Max(MinBufferSize, sizeInBytes);

            BufferDesc desc = BufferDesc.Default(size);

            return Context.Factory.CreateBuffer(in desc);
        }
    }

    public void Return(Buffer buffer)
    {
        lock (this)
        {
            if (buffers.Count >= MaxBufferCount)
            {
                buffer.Dispose();

                return;
            }

            buffers.Add(buffer);
        }
    }

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        lock (this)
        {
            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }

            buffers.Clear();
        }
    }
}
