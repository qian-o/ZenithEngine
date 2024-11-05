using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Sampler(Context context,
                             ref readonly SamplerDescription description) : DeviceResource(context)
{
    public SamplerDescription Description { get; } = description;
}
