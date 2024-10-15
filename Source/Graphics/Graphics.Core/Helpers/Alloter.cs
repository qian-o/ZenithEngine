using System.Runtime.InteropServices;

namespace Graphics.Core.Helpers;

public unsafe class Alloter : DisposableObject
{
    private readonly object _locker = new();
    private readonly List<nint> _allocated = [];

    public byte* Allocate(string value)
    {
        lock (_locker)
        {
            nint ptr = Marshal.StringToHGlobalAnsi(value);

            _allocated.Add(ptr);

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

            _allocated.Add((nint)ptr);

            return ptr;
        }
    }

    public T* Allocate<T>(int length = 1) where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);

        lock (_locker)
        {
            T* ptr = (T*)NativeMemory.Alloc((uint)(sizeof(T) * length));

            _allocated.Add((nint)ptr);

            return ptr;
        }
    }

    public T* Allocate<T>(T value) where T : unmanaged
    {
        T* ptr = Allocate<T>();

        *ptr = value;

        return ptr;
    }

    public T* Allocate<T>(T[] values) where T : unmanaged
    {
        lock (_locker)
        {
            T* ptr = Allocate<T>(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                ptr[i] = values[i];
            }

            return ptr;
        }
    }

    public void Clear()
    {
        lock (_locker)
        {
            foreach (nint ptr in _allocated)
            {
                Marshal.FreeHGlobal(ptr);
            }

            _allocated.Clear();
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
