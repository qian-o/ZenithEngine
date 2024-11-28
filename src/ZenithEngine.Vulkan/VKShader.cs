using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKShader : Shader
{
    public VkShaderModule ShaderModule;

    public VKShader(GraphicsContext context,
                    ref readonly ShaderDesc desc) : base(context, in desc)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (uint)desc.ShaderBytes.Length,
            PCode = (uint*)MemoryAllocator.Alloc(desc.ShaderBytes)
        };

        Context.Vk.CreateShaderModule(Context.Device,
                                      &createInfo,
                                      null,
                                      out ShaderModule).ThrowIfError();

        MemoryAllocator.Free(createInfo.PCode);

        PipelineShaderStageCreateInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = VKFormats.GetShaderStageFlags(desc.Stage),
            Module = ShaderModule,
            PName = MemoryAllocator.AllocUTF8(desc.EntryPoint)
        };
    }

    public PipelineShaderStageCreateInfo PipelineShaderStageCreateInfo { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.ShaderModule, ShaderModule.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyShaderModule(Context.Device, ShaderModule, null);
    }
}
