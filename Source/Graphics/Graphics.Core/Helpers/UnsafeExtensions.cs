﻿namespace Graphics.Core.Helpers;

public static unsafe class UnsafeExtensions
{
    public static T* AsPointer<T>(this ref T value) where T : unmanaged
    {
        fixed (T* ptr = &value)
        {
            return ptr;
        }
    }

    public static T* AsPointer<T>(this T[] array, ulong startIndex = 0) where T : unmanaged
    {
        fixed (T* ptr = &array[startIndex])
        {
            return ptr;
        }
    }
}
