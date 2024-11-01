using Graphics.Core;
using Graphics.Engine.Enums;

namespace Graphics.Engine;

public abstract class Context : DisposableObject
{
    public abstract Backend Backend { get; }

    public abstract void CreateDevice(bool useValidationLayers = false);
}
