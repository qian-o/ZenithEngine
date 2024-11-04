using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Shader(Context context, ref readonly ShaderDescription description) : DeviceResource(context)
{
    public ShaderDescription Description { get; } = description;
}
