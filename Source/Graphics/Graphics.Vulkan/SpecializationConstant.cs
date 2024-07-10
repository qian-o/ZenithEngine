﻿using System.Runtime.CompilerServices;
using Graphics.Core;

namespace Graphics.Vulkan;

public readonly record struct SpecializationConstant
{
    public SpecializationConstant(uint id, ShaderConstantType type, ulong data)
    {
        ID = id;
        Type = type;
        Data = data;
    }

    public SpecializationConstant(uint id, bool value) : this(id, ShaderConstantType.ConstBool, Store(value))
    {
    }

    public SpecializationConstant(uint id, short value) : this(id, ShaderConstantType.ConstInt16, Store(value))
    {
    }

    public SpecializationConstant(uint id, ushort value) : this(id, ShaderConstantType.ConstUInt16, Store(value))
    {
    }

    public SpecializationConstant(uint id, int value) : this(id, ShaderConstantType.ConstInt32, Store(value))
    {
    }

    public SpecializationConstant(uint id, uint value) : this(id, ShaderConstantType.ConstUInt32, Store(value))
    {
    }

    public SpecializationConstant(uint id, long value) : this(id, ShaderConstantType.ConstInt64, Store(value))
    {
    }

    public SpecializationConstant(uint id, ulong value) : this(id, ShaderConstantType.ConstUInt64, Store(value))
    {
    }

    public SpecializationConstant(uint id, float value) : this(id, ShaderConstantType.ConstFloat, Store(value))
    {
    }

    public SpecializationConstant(uint id, double value) : this(id, ShaderConstantType.ConstDouble, Store(value))
    {
    }

    /// <summary>
    /// The constant variable ID.
    /// </summary>
    public uint ID { get; }

    /// <summary>
    /// The type of data stored in this instance. Must be a scalar numeric type.
    /// </summary>
    public ShaderConstantType Type { get; }

    /// <summary>
    /// An 8-byte block storing the contents of the specialization value.
    /// </summary>
    public ulong Data { get; }

    private static unsafe ulong Store<T>(T value)
    {
        ulong ret;
        Unsafe.Write(&ret, value);
        return ret;
    }
}
