using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct ShaderDesc(ShaderStages stage,
                         byte[] shaderBytes,
                         string entryPoint)
{
    public ShaderDesc()
    {
    }

    /// <summary>
    /// The shader stage this instance describes.
    /// </summary>
    public ShaderStages Stage = stage;

    /// <summary>
    /// An array containing the raw shader bytes.
    /// </summary>
    public byte[] ShaderBytes = shaderBytes;

    /// <summary>
    /// The name of the entry point function in the shader module to be used in this stage.
    /// </summary>
    public string EntryPoint = entryPoint;
}
