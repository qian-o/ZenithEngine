using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Shader : DeviceResource
{
    internal Shader(GraphicsDevice graphicsDevice, ref readonly ShaderDescription description) : base(graphicsDevice)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (uint)description.ShaderBytes.Length,
            PCode = (uint*)description.ShaderBytes.AsPointer()
        };

        VkShaderModule shaderModule;
        Vk.CreateShaderModule(Device, &createInfo, null, &shaderModule).ThrowCode();

        Handle = shaderModule;
        Stage = description.Stage;
        EntryPoint = description.EntryPoint;
    }

    internal VkShaderModule Handle { get; }

    public ShaderStages Stage { get; }

    public string EntryPoint { get; }

    protected override void Destroy()
    {
        Vk.DestroyShaderModule(Device, Handle, null);
    }
}
