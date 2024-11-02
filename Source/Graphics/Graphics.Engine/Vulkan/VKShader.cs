﻿using Graphics.Core.Helpers;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKShader : Shader
{
    public VKShader(Context context, ref readonly ShaderDescription description) : base(context, in description)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (uint)description.ShaderBytes.Length,
            PCode = (uint*)description.ShaderBytes.AsPointer()
        };

        VkShader shader;
        Context.Vk.CreateShaderModule(Context.Device, &createInfo, null, &shader).ThrowCode();

        Shader = shader;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkShader Shader { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.ShaderModule, Shader.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyShaderModule(Context.Device, Shader, null);
    }
}
