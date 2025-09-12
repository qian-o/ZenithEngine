using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKQueryHeap : QueryHeap
{
    public VkQueryPool QueryPool;

    public VKQueryHeap(GraphicsContext context,
                       ref readonly QueryHeapDesc desc) : base(context, in desc)
    {
        QueryPoolCreateInfo createInfo = new()
        {
            SType = StructureType.QueryPoolCreateInfo,
            QueryType = VKFormats.GetQueryType(desc.Type),
            QueryCount = desc.Count
        };

        Context.Vk.CreateQueryPool(Context.Device, &createInfo, null, out QueryPool).ThrowIfError();
        Context.Vk.ResetQueryPool(Context.Device, QueryPool, 0, desc.Count);
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public override void GetData(int startIndex, Span<ulong> data)
    {
        Context.Vk.GetQueryPoolResults(Context.Device,
                                       QueryPool,
                                       (uint)startIndex,
                                       (uint)data.Length,
                                       (nuint)(data.Length * sizeof(ulong)),
                                       ref data[0],
                                       sizeof(ulong),
                                       QueryResultFlags.Result64Bit);
    }

    protected override void SetName(string name)
    {
        DebugUtilsObjectNameInfoEXT nameInfo = new()
        {
            SType = StructureType.DebugUtilsObjectNameInfoExt,
            ObjectType = ObjectType.QueryPool,
            ObjectHandle = QueryPool.Handle,
            PObjectName = Allocator.AllocUTF8(name)
        };

        Context.ExtDebugUtils!.SetDebugUtilsObjectName(Context.Device, &nameInfo).ThrowIfError();
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyQueryPool(Context.Device, QueryPool, null);
    }
}
