using SharpMetal.Foundation;
using System;
using System.Text;
using SharpMetal.Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal unsafe class MTLShader : Shader
{
    private SharpMetal.Metal.MTLFunction function;

    public MTLShader(GraphicsContext context,
                     ref readonly ShaderDesc desc) : base(context, in desc)
    {
        // In Metal, shaders are compiled from a library
        // The shader bytes should be compiled Metal library (.metallib) or Metal source
        
        SharpMetal.Metal.MTLLibrary library;
        NSError error = new(IntPtr.Zero);

        // Try to load as source code (most common case)
        string source = Encoding.UTF8.GetString(desc.ShaderBytes);
        library = Context.Device.NewLibrary(new NSString(source), new MTLCompileOptions(IntPtr.Zero), ref error);
        
        if (error != IntPtr.Zero)
        {
            // If source compilation fails, try as precompiled library
            try
            {
                fixed (byte* pBytes = desc.ShaderBytes)
                {
                    error = new(IntPtr.Zero);
                    library = Context.Device.NewLibrary((IntPtr)pBytes, ref error);
                    
                    if (error != IntPtr.Zero)
                    {
                        throw new InvalidOperationException($"Failed to load Metal library: {error.LocalizedDescription}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create Metal library from shader bytes: {ex.Message}");
            }
        }

        Library = library;

        // Get the function from the library
        function = library.NewFunction(new NSString(desc.EntryPoint));
        if (function == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to find function '{desc.EntryPoint}' in Metal library");
        }
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public SharpMetal.Metal.MTLLibrary Library { get; }

    public SharpMetal.Metal.MTLFunction Function => function;

    protected override void SetName(string name)
    {
        Library.Label = new NSString(name);
        function.Label = new NSString(name);
    }

    protected override void Destroy()
    {
        function.Dispose();
        Library.Dispose();
    }
}
