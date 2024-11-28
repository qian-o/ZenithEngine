﻿using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common;

public static class Utils
{
    public static uint GetMipLevels(uint width, uint height)
    {
        return (uint)MathF.Floor(MathF.Log2(MathF.Max(width, height))) + 1;
    }

    public static uint GetMipLevels(uint width, uint height, uint depth)
    {
        return (uint)MathF.Floor(MathF.Log2(MathF.Max(MathF.Max(width, height), depth))) + 1;
    }

    public static void GetMipDimensions(uint width,
                                        uint height,
                                        uint mipLevel,
                                        out uint mipWidth,
                                        out uint mipHeight)
    {
        mipWidth = Math.Max(1, width >> (int)mipLevel);
        mipHeight = Math.Max(1, height >> (int)mipLevel);
    }

    public static void GetMipDimensions(uint width,
                                        uint height,
                                        uint depth,
                                        uint mipLevel,
                                        out uint mipWidth,
                                        out uint mipHeight,
                                        out uint mipDepth)
    {
        mipWidth = Math.Max(1, width >> (int)mipLevel);
        mipHeight = Math.Max(1, height >> (int)mipLevel);
        mipDepth = Math.Max(1, depth >> (int)mipLevel);
    }

    public static uint GetFormatSizeInBytes(ElementFormat format)
    {
        return format switch
        {
            ElementFormat.UByte1 or
            ElementFormat.Byte1 or
            ElementFormat.UByte1Normalized or
            ElementFormat.Byte1Normalized => 1,

            ElementFormat.UByte2 or
            ElementFormat.Byte2 or
            ElementFormat.UByte2Normalized or
            ElementFormat.Byte2Normalized or
            ElementFormat.UShort1 or
            ElementFormat.Short1 or
            ElementFormat.UShort1Normalized or
            ElementFormat.Short1Normalized or
            ElementFormat.Half1 => 2,

            ElementFormat.UByte3 or
            ElementFormat.Byte3 or
            ElementFormat.UByte3Normalized or
            ElementFormat.Byte3Normalized or
            ElementFormat.Half3 => 3,

            ElementFormat.UByte4 or
            ElementFormat.Byte4 or
            ElementFormat.UByte4Normalized or
            ElementFormat.Byte4Normalized or
            ElementFormat.UShort2 or
            ElementFormat.Short2 or
            ElementFormat.UShort2Normalized or
            ElementFormat.Short2Normalized or
            ElementFormat.Half2 or
            ElementFormat.Float1 or
            ElementFormat.UInt1 or
            ElementFormat.Int1 => 4,

            ElementFormat.UShort3 or
            ElementFormat.Short3 or
            ElementFormat.UShort3Normalized or
            ElementFormat.Short3Normalized => 6,

            ElementFormat.UShort4 or
            ElementFormat.Short4 or
            ElementFormat.UShort4Normalized or
            ElementFormat.Short4Normalized or
            ElementFormat.Half4 or
            ElementFormat.Float2 or
            ElementFormat.UInt2 or
            ElementFormat.Int2 => 8,

            ElementFormat.Float3 or
            ElementFormat.UInt3 or
            ElementFormat.Int3 => 12,

            ElementFormat.Float4 or
            ElementFormat.UInt4 or
            ElementFormat.Int4 => 16,

            _ => throw new InvalidOperationException("VertexElementFormat doesn't supported.")
        };
    }

    public static uint CalcAnsiSizeInBytes(string value)
    {
        return (uint)(Encoding.UTF8.GetByteCount(value) + 1);
    }

    public static uint CalcUniSizeInBytes(string value)
    {
        return (uint)(Encoding.Unicode.GetByteCount(value) + 2);
    }

    public static string PtrToStringAnsi(nint ptr)
    {
        return Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
    }

    public static string PtrToStringUni(nint ptr)
    {
        return Marshal.PtrToStringUni(ptr) ?? string.Empty;
    }

    public static T AlignedSize<T>(T size, T alignment) where T : INumberBase<T>, IBitwiseOperators<T, T, T>
    {
        return (size + alignment - T.One) & ~(alignment - T.One);
    }

    public static T Lerp<T>(T start, T end, T value) where T : INumberBase<T>
    {
        return start + ((end - start) * value);
    }
}
