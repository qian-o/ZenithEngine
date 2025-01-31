using Silk.NET.Direct3D12;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXShader : Shader
{
    public ShaderBytecode Shader;

    public DXShader(GraphicsContext context,
                    ref readonly ShaderDesc desc) : base(context, in desc)
    {
        Shader = new()
        {
            PShaderBytecode = Allocator.Alloc(desc.ShaderBytes),
            BytecodeLength = (nuint)desc.ShaderBytes.Length
        };
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
