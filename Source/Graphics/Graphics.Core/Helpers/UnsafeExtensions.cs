using System.Runtime.CompilerServices;

namespace Graphics.Core;

public static unsafe class UnsafeExtensions
{
    public static T* AsPointer<T>(this ref T value) where T : unmanaged
    {
        return (T*)Unsafe.AsPointer(ref value);
    }

    public static T* AsPointer<T>(this T[] array, ulong startIndex = 0) where T : unmanaged
    {
        return (T*)Unsafe.AsPointer(ref array[startIndex]);
    }
}
