using Graphics.Core;

namespace Graphics.Vulkan;

public struct ShaderDescription(ShaderStages stage, byte[] shaderBytes, string entryPoint) : IEquatable<ShaderDescription>
{
    /// <summary>
    /// The shader stage this instance describes.
    /// </summary>
    public ShaderStages Stage { get; set; } = stage;

    /// <summary>
    /// An array containing the raw shader bytes.
    /// Shader bytecode in SPIR-V format or UTF8-encoded GLSL source code.
    /// </summary>
    public byte[] ShaderBytes { get; set; } = shaderBytes;

    /// <summary>
    /// The name of the entry point function in the shader module to be used in this stage.
    /// </summary>
    public string EntryPoint { get; set; } = entryPoint;

    public readonly bool Equals(ShaderDescription other)
    {
        return Stage == other.Stage
               && ShaderBytes.SequenceEqual(other.ShaderBytes)
               && EntryPoint == other.EntryPoint;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Stage.GetHashCode(), ShaderBytes.GetHashCode(), EntryPoint.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is ShaderDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Stage: {Stage}, ShaderBytes: {ShaderBytes}, EntryPoint: {EntryPoint}";
    }

    public static bool operator ==(ShaderDescription left, ShaderDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderDescription left, ShaderDescription right)
    {
        return !(left == right);
    }
}
