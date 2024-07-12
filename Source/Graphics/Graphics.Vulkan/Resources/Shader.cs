using System.Runtime.CompilerServices;
using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Shader : DeviceResource
{
    private readonly VkShaderModule _shaderModule;
    private readonly ShaderStages _stage;
    private readonly string _entryPoint;

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
        _stage = description.Stage;
        _entryPoint = description.EntryPoint;
    }

    internal VkShaderModule Handle => _shaderModule;

    public ShaderStages Stage => _stage;

    public string EntryPoint => _entryPoint;

    protected override void Destroy()
    {
        Vk.DestroyShaderModule(Device, _shaderModule, null);
    }
}
