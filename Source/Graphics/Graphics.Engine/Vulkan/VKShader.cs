using Graphics.Core.Helpers;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKShader : Shader
{
    public VKShader(Context context, ref readonly ShaderDescription description) : base(context, in description)
    {
        VKContext vkContext = (VKContext)context;

        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (uint)description.ShaderBytes.Length,
            PCode = (uint*)description.ShaderBytes.AsPointer()
        };

        VkShader shader;
        vkContext.Vk.CreateShaderModule(vkContext.Device, &createInfo, null, &shader).ThrowCode("Failed to create shader module");

        Shader = shader;
    }

    public VkShader Shader { get; }

    protected override void SetName(string name)
    {
        VKContext vkContext = (VKContext)Context;

        vkContext.Debug?.SetObjectName(vkContext.Device, ObjectType.ShaderModule, Shader.Handle, name);
    }

    protected override void Destroy()
    {
        VKContext vkContext = (VKContext)Context;

        vkContext.Vk.DestroyShaderModule(vkContext.Device, Shader, null);
    }
}
