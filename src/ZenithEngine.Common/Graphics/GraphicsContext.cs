using System.Reflection;
using System.Runtime.CompilerServices;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public abstract unsafe class GraphicsContext : DisposableObject
{
    public static readonly Version VulkanApiVersion = new(1, 3, 0);

    public abstract Backend Backend { get; }

    public abstract DeviceCapabilities Capabilities { get; }

    public abstract ResourceFactory Factory { get; }

    protected CommandProcessor? CopyProcessor { get; private set; }

    public void CreateDevice(bool useDebugLayer = false)
    {
        CreateDeviceInternal(useDebugLayer);

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
        if (buffer.Desc.Usage.HasFlag(BufferUsage.Dynamic))
        {
            MappedResource mappedResource = MapMemory(buffer, MapMode.Write);

            Unsafe.CopyBlock((void*)(mappedResource.Data + destinationOffsetInBytes),
                             (void*)source,
                             sourceSizeInBytes);

            UnmapMemory(buffer);
        }
        else
        {
            CommandBuffer commandBuffer = CopyProcessor!.CommandBuffer();

            commandBuffer.Begin();

            commandBuffer.UpdateBuffer(buffer,
                                       source,
                                       sourceSizeInBytes,
                                       destinationOffsetInBytes);

            commandBuffer.End();

            commandBuffer.Commit();
        }
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
        CopyProcessor!.Submit(false);
        CopyProcessor.WaitIdle();
    }

    protected override void Destroy()
    {
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
