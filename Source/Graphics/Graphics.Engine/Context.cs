using Graphics.Core;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan;

namespace Graphics.Engine;

public abstract class Context : DisposableObject
{
    public abstract Backend Backend { get; }

    public abstract DeviceCapabilities Capabilities { get; }

    public abstract void CreateDevice(bool useValidationLayers = false);

    public static Context Create(Backend backend)
    {
        return backend switch
        {
            Backend.Vulkan => new VKContext(),
            _ => throw new NotSupportedException()
        };
    }
}
