using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class FrameBuffer(GraphicsContext context,
                                  ref readonly FrameBufferDesc desc) : GraphicsResource(context)
{
    public FrameBufferDesc Desc { get; } = desc;

    /// <summary>
    /// Get the color attachment size at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public abstract Vector2D<uint> this[int index] { get; }

    /// <summary>
    /// Get the output description for graphics pipeline creation.
    /// </summary>
    public abstract OutputDesc Output { get; }
}
