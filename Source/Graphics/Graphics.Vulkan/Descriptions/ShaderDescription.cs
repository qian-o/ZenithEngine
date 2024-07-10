using Graphics.Core;

namespace Graphics.Vulkan;

public readonly record struct ShaderDescription
{
    public ShaderDescription(ShaderStages stage, byte[] shaderBytes, string entryPoint)
    {
        Stage = stage;
        ShaderBytes = shaderBytes;
        EntryPoint = entryPoint;
    }

    /// <summary>
    /// The shader stage this instance describes.
    /// </summary>
    public ShaderStages Stage { get; }

    /// <summary>
    /// An array containing the raw shader bytes.
    /// Shader bytecode in SPIR-V format or UTF8-encoded GLSL source code.
    /// </summary>
    public byte[] ShaderBytes { get; } = [];

    /// <summary>
    /// The name of the entry point function in the shader module to be used in this stage.
    /// </summary>
    public string EntryPoint { get; } = string.Empty;
}
