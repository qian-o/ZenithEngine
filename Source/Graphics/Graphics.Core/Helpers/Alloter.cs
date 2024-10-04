using System.Runtime.InteropServices;

namespace Graphics.Core.Helpers;

public unsafe class Alloter : DisposableObject
{
    private readonly object _locker = new();
    private readonly Dictionary<string, nint> _marshalAllocated = [];
    private readonly List<nint> _nativeAllocated = [];

    public byte* Allocate(string value)
    {
        lock (_locker)
        {
            if (!_marshalAllocated.TryGetValue(value, out nint ptr))
            {
                ptr = Marshal.StringToHGlobalAnsi(value);

                _marshalAllocated.Add(value, ptr);
            }

            return (byte*)ptr;
        }
    }

    public byte** Allocate(string[] values)
    {
        lock (_locker)
        {
            byte** ptr = (byte**)NativeMemory.Alloc((uint)(nint.Size * values.Length));

            for (int i = 0; i < values.Length; i++)
            {
                ptr[i] = Allocate(values[i]);
            }

            _nativeAllocated.Add((nint)ptr);

            return ptr;
        }
    }

    public T* Allocate<T>(T value) where T : unmanaged
    {
        lock (_locker)
        {
            T* ptr = (T*)NativeMemory.Alloc((uint)sizeof(T));

            *ptr = value;

            _nativeAllocated.Add((nint)ptr);

            return ptr;
        }
    }

    public T* Allocate<T>(T[] values) where T : unmanaged
    {
        lock (_locker)
        {
            T* ptr = (T*)NativeMemory.Alloc((uint)(sizeof(T) * values.Length));

            for (int i = 0; i < values.Length; i++)
            {
                ptr[i] = values[i];
            }

            _nativeAllocated.Add((nint)ptr);

            return ptr;
        }
    }

    public void Free(string value)
    {
        lock (_locker)
        {
            if (_marshalAllocated.TryGetValue(value, out nint ptr))
            {
                Marshal.FreeHGlobal(ptr);

                _marshalAllocated.Remove(value);
            }
        }
    }

    public void Free(void* marshalPtr)
    {
        lock (_locker)
        {
            if (_nativeAllocated.Remove((nint)marshalPtr))
            {
                NativeMemory.Free(marshalPtr);
            }
        }
    }

    public void Clear()
    {
        lock (_locker)
        {
            foreach (nint ptr in _marshalAllocated.Values)
            {
                Marshal.FreeHGlobal(ptr);
            }

            foreach (nint ptr in _nativeAllocated)
            {
                NativeMemory.Free((void*)ptr);
            }

            _marshalAllocated.Clear();
            _nativeAllocated.Clear();
        }
    }

    protected override void Destroy()
    {
        Clear();
    }

    public static unsafe string GetString(byte* stringPtr)
    {
        return Marshal.PtrToStringAnsi((nint)stringPtr) ?? string.Empty;
    }

    public static unsafe string[] GetStrings(byte** stringsPtr, int count)
    {
        string[] strings = new string[count];

        for (int i = 0; i < count; i++)
        {
            strings[i] = GetString((byte*)Marshal.ReadIntPtr((nint)stringsPtr, i * nint.Size));
        }

        return strings;
    }
}
