using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Shader : DeviceResource
{
    private readonly VkShaderModule _shaderModule;

    internal Shader(GraphicsDevice graphicsDevice, ref readonly ShaderDescription description) : base(graphicsDevice)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (uint)description.ShaderBytes.Length,
            PCode = (uint*)Unsafe.AsPointer(ref description.ShaderBytes[0])
        };

        VkShaderModule shaderModule;
        Vk.CreateShaderModule(Device, &createInfo, null, &shaderModule).ThrowCode();

        _shaderModule = shaderModule;
    }

    internal VkShaderModule Handle => _shaderModule;

    protected override void Destroy()
    {
        Vk.DestroyShaderModule(Device, _shaderModule, null);
    }
}
