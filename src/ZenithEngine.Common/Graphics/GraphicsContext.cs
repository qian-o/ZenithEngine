using System.Reflection;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public abstract class GraphicsContext : DisposableObject
{
    public abstract Backend Backend { get; }

    public abstract DeviceCapabilities Capabilities { get; }

    public abstract ResourceFactory Factory { get; }

    public BufferAllocator? BufferAllocator { get; private set; }

    public CommandProcessor? CopyProcessor { get; private set; }

    protected Lock Lock { get; } = new Lock();

    public void CreateDevice(bool useDebugLayer = false)
    {
        CreateDeviceInternal(useDebugLayer);

        BufferAllocator = new BufferAllocator(this);
        CopyProcessor = Factory.CreateCommandProcessor(CommandProcessorType.Copy);
    }

    public abstract void CreateDeviceInternal(bool useDebugLayer);

    public abstract MappedResource MapMemory(Buffer buffer, MapMode mode);

    public abstract void UnmapMemory(Buffer buffer);

    public void UpdateBuffer(Buffer buffer,
                             nint source,
                             uint sourceSizeInBytes,
                             uint destinationOffsetInBytes = 0)
    {
        CommandBuffer commandBuffer = CopyProcessor!.CommandBuffer();

        commandBuffer.Begin();

        commandBuffer.UpdateBuffer(buffer, source, sourceSizeInBytes, destinationOffsetInBytes);

        commandBuffer.End();

        commandBuffer.Commit();
    }

    public void UpdateTexture(Texture texture,
                              nint source,
                              uint sourceSizeInBytes,
                              TextureRegion region)
    {
        CommandBuffer commandBuffer = CopyProcessor!.CommandBuffer();

        commandBuffer.Begin();

        commandBuffer.UpdateTexture(texture, source, sourceSizeInBytes, region);

        commandBuffer.End();

        commandBuffer.Commit();
    }

    public void SyncCopyTasks()
    {
        if (CopyProcessor is null)
        {
            throw new ZenithEngineException(Backend, "Device not created.");
        }

        Lock.Enter();

        CopyProcessor.Submit(false);
        CopyProcessor.WaitIdle();

        BufferAllocator!.Release();

        Lock.Exit();
    }

    protected override void Destroy()
    {
        BufferAllocator?.Dispose();
        BufferAllocator = null;

        CopyProcessor?.Dispose();
        CopyProcessor = null;
    }

    protected abstract void DestroyInternal();

    public static GraphicsContext Create(Backend backend)
    {
        return backend switch
        {
            Backend.Vulkan => CreateInstance<GraphicsContext>("ZenithEngine.Vulkan", "VKGraphicsContext"),
            _ => throw new NotSupportedException()
        };
    }

    private static T CreateInstance<T>(string assemblyName, string typeName, params object[] args)
    {
        Assembly assembly = Assembly.Load(assemblyName);
        typeName = $"{assemblyName}.{typeName}";

        if (args.Length == 0)
        {
            return (T)assembly.CreateInstance(typeName)!;
        }
        else
        {
            return (T)assembly.CreateInstance(typeName,
                                              true,
                                              BindingFlags.Default,
                                              null,
                                              args,
                                              null,
                                              null)!;
        }
    }
}
