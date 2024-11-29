namespace ZenithEngine.ShaderCompiler;

internal unsafe struct VTable
{
    public delegate* unmanaged[Stdcall]<ObjectHandle*, Guid*, void**, int> QueryInterface;

    public delegate* unmanaged[Stdcall]<ObjectHandle*, ulong> AddRef;

    public delegate* unmanaged[Stdcall]<ObjectHandle*, ulong> RemoveRef;
}
