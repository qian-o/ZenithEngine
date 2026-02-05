using Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal unsafe class MTLShader : Shader
{
    private IMTLFunction? function;

    public MTLShader(GraphicsContext context,
                     ref readonly ShaderDesc desc) : base(context, in desc)
    {
        // In Metal, shaders are compiled from a library
        // The shader bytes should be compiled Metal library (.metallib) or Metal source
        
        IMTLLibrary? library = null;

        try
        {
            // Try to load as a precompiled library
            fixed (byte* pBytes = desc.ShaderBytes)
            {
                using MTLDispatchData dispatchData = MTLDispatchData.CreateWithBytes((nint)pBytes, (nuint)desc.ShaderBytes.Length);
                library = Context.Device.CreateLibrary(dispatchData, out NSError? error);
                
                if (library is null && error is not null)
                {
                    throw new InvalidOperationException($"Failed to create Metal library: {error.LocalizedDescription}");
                }
            }
        }
        catch
        {
            // If loading as library fails, try as source code
            string source = System.Text.Encoding.UTF8.GetString(desc.ShaderBytes);
            library = Context.Device.CreateLibrary(source, null, out NSError? error);
            
            if (library is null && error is not null)
            {
                throw new InvalidOperationException($"Failed to compile Metal shader: {error.LocalizedDescription}");
            }
        }

        if (library is null)
        {
            throw new InvalidOperationException("Failed to create Metal library from shader bytes");
        }

        Library = library;

        // Get the function from the library
        function = library.CreateFunction(desc.EntryPoint);
        if (function is null)
        {
            throw new InvalidOperationException($"Failed to find function '{desc.EntryPoint}' in Metal library");
        }
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public IMTLLibrary Library { get; }

    public IMTLFunction Function => function ?? throw new InvalidOperationException("Shader function is null");

    protected override void SetName(string name)
    {
        Library.Label = name;
        if (function is not null)
        {
            function.Label = name;
        }
    }

    protected override void Destroy()
    {
        function?.Dispose();
        Library.Dispose();
    }
}
