using System.Runtime.CompilerServices;

namespace Graphics.Core;

public static unsafe class UnsafeHelpers
{
    public static T* AsPointer<T>(ref T value) where T : unmanaged
    {
        return (T*)Unsafe.AsPointer(ref value);
    }

    public static T* AsPointer<T>(this T[] array, ulong offset = 0) where T : unmanaged
    {
        return (T*)Unsafe.AsPointer(ref array[offset]);
    }
}
