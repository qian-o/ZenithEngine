using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public abstract class GraphicsContext : DisposableObject
{
    public abstract Backend Backend { get; }

    public abstract DeviceCapabilities Capabilities { get; }

    public abstract ResourceFactory Factory { get; }

    public abstract void CreateDevice(bool useDebugLayer = false);

    public abstract void UpdateBuffer(Buffer buffer,
                                      nint source,
                                      uint sourceSizeInBytes,
                                      uint destinationOffsetInBytes = 0);

    public abstract void UpdateTexture(Texture texture,
                                       nint source,
                                       uint sourceSizeInBytes,
                                       TextureRegion region);

    public abstract MappedResource MapMemory(Buffer buffer, MapMode mode);

    public abstract void UnmapMemory(Buffer buffer);

    public abstract void SyncCopyTasks();

    public static GraphicsContext Create(Backend backend)
    {
        return backend switch
        {
            _ => throw new NotSupportedException()
        };
    }
}
