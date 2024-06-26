using System.Runtime.InteropServices;

namespace Graphics.Core;

public class StringAlloter : DisposableObject
{
    private readonly List<nint> _allocated = [];

    public nint Allocate(string value)
    {
        nint ptr = Marshal.StringToHGlobalAnsi(value);

        _allocated.Add(ptr);

        return ptr;
    }

    public nint Allocate(string[] values)
    {
        nint ptr = Marshal.AllocHGlobal(nint.Size * values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            nint strPtr = Allocate(values[i]);

            Marshal.WriteIntPtr(ptr, i * nint.Size, strPtr);
        }

        _allocated.Add(ptr);

        return ptr;
    }

    public void Clear()
    {
        foreach (nint ptr in _allocated)
        {
            Marshal.FreeHGlobal(ptr);
        }

        _allocated.Clear();
    }

    protected override void Destroy()
    {
        foreach (nint ptr in _allocated)
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public static unsafe string GetString(void* stringPtr)
    {
        return Marshal.PtrToStringAnsi((nint)stringPtr) ?? string.Empty;
    }

    public static unsafe string[] GetStrings(void* stringsPtr, int count)
    {
        string[] strings = new string[count];

        for (int i = 0; i < count; i++)
        {
            strings[i] = GetString((void*)Marshal.ReadIntPtr((nint)stringsPtr, i * nint.Size));
        }

        return strings;
    }
}
