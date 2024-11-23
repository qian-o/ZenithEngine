using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKShader : Shader
{
    public VkShaderModule ShaderModule;
    public PipelineShaderStageCreateInfo ShaderStageCreateInfo;

    public VKShader(GraphicsContext context,
                    ref readonly ShaderDesc desc) : base(context, in desc)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (uint)desc.ShaderBytes.Length,
            PCode = (uint*)Unsafe.AsPointer(ref desc.ShaderBytes[0])
        };

        Context.Vk.CreateShaderModule(Context.Device, &createInfo, null, out ShaderModule).ThrowIfError();

        ShaderStageCreateInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = VKFormats.GetShaderStageFlags(desc.Stage),
            Module = ShaderModule,
            PName = MemoryAllocator.AllocAnsi(desc.EntryPoint)
        };
    }

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.ShaderModule, ShaderModule.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyShaderModule(Context.Device, ShaderModule, null);
    }
}
