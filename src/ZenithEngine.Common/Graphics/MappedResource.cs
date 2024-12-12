using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public readonly struct MappedResource(Buffer buffer,
                                      MapMode mode,
                                      nint data,
                                      uint sizeInBytes)
{
    public readonly Buffer Buffer = buffer;

    public readonly MapMode Mode = mode;

    public readonly nint Data = data;

    public readonly uint SizeInBytes = sizeInBytes;
}
