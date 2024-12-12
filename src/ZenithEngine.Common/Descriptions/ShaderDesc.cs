using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct ShaderDesc
{
    /// <summary>
    /// The shader stage this instance describes.
    /// </summary>
    public ShaderStages Stage;

    /// <summary>
    /// An array containing the raw shader bytes.
    /// Shader bytecode in SPIR-V format.
    /// </summary>
    public byte[] ShaderBytes;

    /// <summary>
    /// The name of the entry point function in the shader module to be used in this stage.
    /// </summary>
    public string EntryPoint;

    public static ShaderDesc Default(ShaderStages stage,
                                     byte[] shaderBytes,
                                     string entryPoint = "main")
    {
        return new()
        {
            Stage = stage,
            ShaderBytes = shaderBytes,
            EntryPoint = entryPoint
        };
    }
}
