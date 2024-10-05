using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class ShaderTable : VulkanObject<ulong>
{
    public ShaderTable(VulkanResources vkRes, Pipeline pipeline, ref readonly RaytracingPipelineDescription description) : base(vkRes, ObjectType.Buffer)
    {
        uint handleSize = vkRes.RayTracingPipelineProperties.ShaderGroupHandleSize;
        uint handleAlignment = vkRes.RayTracingPipelineProperties.ShaderGroupHandleAlignment;
        uint handleSizeAligned = Util.AlignedSize(handleSize, handleAlignment);

        string raygenShaderName = description.Shaders.RayGenerationShader.Name;
        string[] missShaderNames = description.Shaders.MissShader.Select(x => x.Name).ToArray();
        string[] hitGroupNames = description.HitGroups.Where(item => item.Type != HitGroupType.General)
                                                      .Select(x => x.Name)
                                                      .ToArray();

        uint length = 1 + (uint)missShaderNames.Length + (uint)hitGroupNames.Length;

        uint size = handleSizeAligned * length;

        byte[] shaderHandleStorage = new byte[size];
        VkRes.KhrRayTracingPipeline.GetRayTracingShaderGroupHandles(VkRes.VkDevice, pipeline.Handle, 0, length, size, shaderHandleStorage.AsPointer());

        byte[] raygenShaderHandleStorage = shaderHandleStorage.AsSpan(0, (int)handleSizeAligned).ToArray();
        byte[] missShaderHandleStorage = shaderHandleStorage.AsSpan((int)handleSizeAligned, (int)handleSizeAligned * missShaderNames.Length).ToArray();
        byte[] hitGroupHandleStorage = shaderHandleStorage.AsSpan((int)handleSizeAligned * (1 + missShaderNames.Length), (int)handleSizeAligned * hitGroupNames.Length).ToArray();

        DeviceBuffer raygenShaderHandleBuffer = new(VkRes,
                                                    BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.ShaderBindingTableBitKhr,
                                                    (uint)raygenShaderHandleStorage.Length,
                                                    true);
        vkRes.GraphicsDevice.UpdateBuffer(raygenShaderHandleBuffer, 0, raygenShaderHandleStorage);

        DeviceBuffer missShaderHandleBuffer = new(VkRes,
                                                  BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.ShaderBindingTableBitKhr,
                                                  (uint)missShaderHandleStorage.Length,
                                                  true);
        vkRes.GraphicsDevice.UpdateBuffer(missShaderHandleBuffer, 0, missShaderHandleStorage);

        DeviceBuffer hitGroupHandleBuffer = new(VkRes,
                                                BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.ShaderBindingTableBitKhr,
                                                (uint)hitGroupHandleStorage.Length,
                                                true);
        vkRes.GraphicsDevice.UpdateBuffer(hitGroupHandleBuffer, 0, hitGroupHandleStorage);

        Handle = handleSizeAligned;
        RaygenShaderHandleBuffer = raygenShaderHandleBuffer;
        MissShaderHandleBuffer = missShaderHandleBuffer;
        HitGroupHandleBuffer = hitGroupHandleBuffer;
    }

    internal override ulong Handle { get; }

    internal DeviceBuffer RaygenShaderHandleBuffer { get; }

    internal DeviceBuffer MissShaderHandleBuffer { get; }

    internal DeviceBuffer HitGroupHandleBuffer { get; }

    internal override ulong[] GetHandles()
    {
        return
        [
            RaygenShaderHandleBuffer.Handle.Handle,
            MissShaderHandleBuffer.Handle.Handle,
            HitGroupHandleBuffer.Handle.Handle
        ];
    }

    protected override void Destroy()
    {
        RaygenShaderHandleBuffer.Dispose();
        MissShaderHandleBuffer.Dispose();
        HitGroupHandleBuffer.Dispose();
    }
}
