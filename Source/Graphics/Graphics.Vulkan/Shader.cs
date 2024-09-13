using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Shader : VulkanObject<VkShaderModule>
{
    internal Shader(VulkanResources vkRes, ref readonly ShaderDescription description) : base(vkRes, ObjectType.ShaderModule)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (uint)description.ShaderBytes.Length,
            PCode = (uint*)description.ShaderBytes.AsPointer()
        };

        VkShaderModule shaderModule;
        VkRes.Vk.CreateShaderModule(VkRes.GetDevice(), &createInfo, null, &shaderModule).ThrowCode();

        Handle = shaderModule;
        Stage = description.Stage;
        EntryPoint = description.EntryPoint;
    }

    internal override VkShaderModule Handle { get; }

    internal ShaderStages Stage { get; }

    internal string EntryPoint { get; }

    protected override void Destroy()
    {
        VkRes.Vk.DestroyShaderModule(VkRes.GetDevice(), Handle, null);
    }

    internal override ulong[] GetHandles()
    {
        throw new NotImplementedException();
    }
}
